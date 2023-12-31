﻿open Fli
open System
open System.IO
open FSharp.SystemCommandLine
open System.Runtime.InteropServices

type Language =
  | CSharp
  | FSharp

type Command = {
  Language: Language
  OutputDirectory: DirectoryInfo option
  ProjectName: string option
  ProjectTemplate: string option
  IncludeTests: bool
  IncludeFormatter: bool
}

let shell =
  if
    RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
    || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
  then
    BASH
  else
    CMD

let strToLanguage language =
  let comparison = StringComparison.CurrentCultureIgnoreCase
  let eq a = String.Equals(a, language, comparison)

  if eq "c#" || eq "csharp" then
    CSharp
  elif eq "f#" || eq "fsharp" then
    FSharp
  else
    failwith "Unexpected language. Available options: 'c#', 'csharp', 'f#', 'fsharp'"

let iterAndReturn f option =
  match option with
  | None -> None
  | Some value ->
    f value
    Some value

let createIfNotExists (dir: DirectoryInfo) =
  if not dir.Exists then
    dir.Create()

let printErrorAndReturnExitCode (output: Output) =
  Output.printError output
  Output.toExitCode output

let createProject command =
  let currentDirectory = Directory.CreateDirectory(Directory.GetCurrentDirectory())

  let outputDirectory =
    command.OutputDirectory
    |> iterAndReturn createIfNotExists
    |> Option.defaultValue currentDirectory

  let defaultProjectName = outputDirectory |> fun dir -> dir.Name
  let projectName = command.ProjectName |> Option.defaultValue defaultProjectName
  let projectTemplate = command.ProjectTemplate |> Option.defaultValue "console"

  let languageString =
    match command.Language with
    | CSharp -> "C#"
    | FSharp -> "F#"

  let sourceDirectory = $"./src/{projectName}"
  let testsDirectory = $"./tests/{projectName}.Tests"

  let toolManifestExists =
    File.Exists(Path.Combine(outputDirectory.FullName, ".config", "dotnet-tools.json"))

  let command =
    [
      $"dotnet new sln --name {projectName}"
      $"dotnet new {projectTemplate} -lang {languageString} --output {sourceDirectory}"
      $"dotnet sln add {sourceDirectory}"

      if command.IncludeFormatter then
        if not toolManifestExists then
          "dotnet new tool-manifest"

        match command.Language with
        | CSharp -> "dotnet tool install csharpier"
        | FSharp -> "dotnet tool install fantomas"

      if command.IncludeTests then
        match command.Language with
        | CSharp -> $"dotnet new xunit --output {testsDirectory}"
        | FSharp ->
          $"dotnet new console -lang F# --output {testsDirectory}"
          $"dotnet add {testsDirectory} package Expecto"

        $"dotnet add {testsDirectory} reference {sourceDirectory}"
        $"dotnet sln add {testsDirectory}"
    ]
    |> String.concat " && "

  cli {
    Shell shell
    WorkingDirectory(outputDirectory.FullName)
    Command command
  }
  |> Command.execute
  |> printErrorAndReturnExitCode

[<EntryPoint>]
let main args =
  rootCommand args {
    description "Initializes a new .NET solution"

    inputs (
      Input.Option<string>([ "-lang"; "--language" ], "The language of the project you'd like to create (c#, f#)."),
      Input.OptionMaybe<DirectoryInfo>(
        [ "-o"; "--output" ],
        "The root directory of the solution and project(s) you'd like to create."
      ),
      Input.OptionMaybe<string>("--name", "The name of the solution and project you'd like to create."),
      Input.OptionMaybe<string>("--template", "The dotnet template of the project you'd like to create."),
      Input.Option<bool>("--includeTests", "Include a test project."),
      Input.Option<bool>("--includeFormatter", "Include a formatting tool (fantomas or csharpier).")
    )

    setHandler (fun (language, directoryInfo, name, template, tests, formatter) ->
      createProject {
        Language = strToLanguage language
        OutputDirectory = directoryInfo
        ProjectName = name
        ProjectTemplate = template
        IncludeTests = tests
        IncludeFormatter = formatter
      })
  }
