
(************************************************
* F#MAKE Build skript DM7_P-PLUS_Integration
*
* aufgerufen durch 'build.cmd [target]'
*
* s. http://fsharp.github.io/FAKE/index.html
************************************************)

#r @"_build_tools/FAKE/tools/FakeLib.dll"
#r "System.Xml.Linq.dll"

open Fake

// ------------------------------------ vv Konfiguration

let product = "DM7_PPLUS_Integration"

let buildnumber = match buildServer with
                  | Jenkins -> jenkinsBuildNumber.PadLeft(5, '0')
                  | _ -> "00000"


let branch = environVarOrDefault "branch" "Interne_Entwicklungsversion"
let releasing = branch="master"
let pureversion = System.IO.File.ReadAllText("./version.txt").Trim()+"."+buildnumber+".0"
let version = pureversion + (if releasing then "" else "-preview")

let pack_basedir = "./_pack"
let deploy_basedir = "./_deploy"

trace <| "Building version "+version
trace <| if releasing then "RELEASE CANDIDATE BUILD" else "TEST BUILD"

// ------------------------------------ ^^ Konfiguration
//                                      ||
// ------------------------------------ vv Helper

let empty () = ()

let load_text_file (filename:string) =
    let reader = new System.IO.StreamReader(filename)
    let text = reader.ReadToEnd()
    reader.Dispose()
    text

let write_text_file (text:string) (filename:string) =
    let writer = new System.IO.StreamWriter(filename)
    writer.WriteLine text
    writer.Dispose()

open System.Text.RegularExpressions
open System.IO
exception UnbehandelteTODORelease of string

let file_lines_containing_string searchString filePath =
    File.ReadLines filePath
    |> Seq.filter (fun line -> Regex.IsMatch(line, searchString, RegexOptions.IgnoreCase))
    |> Seq.map (fun line -> filePath + ": " + line)

let get_lines orte funktion searchString =
    orte
    |> Seq.map (fun ort -> funktion searchString ort)
    |> Seq.concat

let ends_with (name:string) (endings:string Set) =
    Set.contains (Path.GetExtension(name)) endings

let rec appearances_of_string_in_directory str dir =
    let subdirs = Directory.EnumerateDirectories(dir) |> Seq.filter (fun name -> name.StartsWith("."))
    let subfiles = Directory.EnumerateFiles(dir) |> Seq.filter (fun x -> ends_with x (set [".cs"; ".fs"; ".fsx"; ".xml"]))

    let files_lines_containing_string = get_lines subfiles file_lines_containing_string str
    let dirs_lines_contain_string = get_lines subdirs appearances_of_string_in_directory str
    Seq.toList files_lines_containing_string @ Seq.toList dirs_lines_contain_string


// ------------------------------------ ^^ Helper
//                                      ||
// ------------------------------------ vv Tasks


let mutable option_force_build = false
let mutable option_superfast_build = false

let do_build1 name output properties projects target =
    MSBuildWithProjectProperties output target properties projects
    |> Seq.map (fun line -> ("build ["+name+"] | "+line))

Target "TodoRelease" ( fun _ ->
        // "TODO " muss getrennt von "Release" sein, sonst findet der Builder immer ein unbehandeltes TODO hier
        let searchString = "TODO " + "Release"
        let appearances_of_string = appearances_of_string_in_directory searchString ".\ "
        printfn "%A" appearances_of_string
        if appearances_of_string.IsEmpty
        then
            printfn "%s" ("Keine TODO " + "Release in Solution.")
        else
            if (not option_force_build)
            then raise (UnbehandelteTODORelease("TODO " + "Release existieren noch im Solution."))
            else printfn "%s" ("TODO " + "Release in Solution, aber durch BUILD option deaktiviert!")
)

Target "Clean" ( fun _ ->
   CleanDirs [deploy_basedir;pack_basedir]

   let props =
      if option_superfast_build
        then (fun _ -> [("MaxCpuCount","1") ; ("BuildInParallel","false") ; ("Verbosity", "Quiet")])
        else (fun _ -> [("MaxCpuCount","1") ; ("BuildInParallel","false")])

   do_build1 product "" props [("./DM7_PPLUS_Integration.sln")] (if option_superfast_build then "Superfast" else "Build")
   |> Log ""
)

Target "RestorePackages" ( fun _ ->
  if (option_superfast_build)
    then trace "skipping due to SUPERFAST!"
    else RestorePackages() )

Target "BuildInfo" (fun _ ->
    let buildinfo_template = load_text_file "./DM7_PPLUS_Integration/Version.cs.txt"
    let buildinfo = buildinfo_template.Replace("$$VERSION$$", version);
    write_text_file buildinfo "./DM7_PPLUS_Integration/Version.gen.cs"
    let assemblyinfo = sprintf "[assembly: System.Reflection.AssemblyVersion(\"%s\")]\r\n[assembly: System.Reflection.AssemblyFileVersion(\"%s\")]" pureversion pureversion
    write_text_file assemblyinfo "./DM7_PPLUS_Integration/Properties/Assemblyinfo.gen.cs"
)

Target "Build" ( fun _ ->
    let props =
      if option_superfast_build
        then (fun _ -> [("MaxCpuCount","1") ; ("BuildInParallel","false") ; ("Verbosity", "Quiet")])
        else (fun _ -> [("MaxCpuCount","1") ; ("BuildInParallel","false")])

    do_build1 product "" props [("./DM7_PPLUS_Integration.sln")] (if option_superfast_build then "Superfast" else "Build")
    |> Log ""
)

let do_zip sourcedir zipfile =
    !! (sourcedir @@ "**/*.*")
    |> Zip sourcedir zipfile

Target "PackDeployables" (fun _ ->

  CleanDir deploy_basedir

  CleanDir pack_basedir

  CleanDir "./nuget"
  NuGetPackDirectly (fun p -> {p with Version=version}) "./DM7_PPLUS_Integration.nuspec"
  FileHelper.CopyFiles pack_basedir (!!"./nuget/*.nupkg")
  FileHelper.CopyFiles deploy_basedir (!!"./nuget/*.nupkg")
  CleanDir "./nuget"


  CleanDirs [pack_basedir@@"Quellcode";pack_basedir@@"Demo"]
  FileHelper.CopyFiles (pack_basedir@@"Demo") ["./README.MD";"./LICENSE"]
  FileHelper.CopyRecursive ("./Demo_Implementierung/Bin/Debug") (pack_basedir@@"Demo") true |> ignore
  FileHelper.CopyRecursive ("./PPLUS_Demo_Server/Bin/Debug") (pack_basedir@@"Demo") true |> ignore
  FileHelper.DeleteFiles (!!((pack_basedir@@"Demo") @@ "*.xml"))
  FileHelper.DeleteFiles (!!((pack_basedir@@"Demo") @@ "*.pdb"))
  FileHelper.CopyFiles (pack_basedir@@"Quellcode") ["./README.MD";"./LICENSE";"./Demo_Implementierung/DemoClientImplementierung.cs"]
  FileHelper.CopyRecursive ("./DM7_PPLUS_Integration/Public") (pack_basedir@@"Quellcode") true |> ignore
  do_zip pack_basedir (deploy_basedir @@ (product + "_" + version+".zip"))

  CleanDir pack_basedir
)


Target "Init" empty
Target "Default" empty

"Init"
  ==> "TodoRelease"
  ==> "BuildInfo"
  ==> "RestorePackages"  
  ==> "Clean"
  ==> "Build"
  ==> "PackDeployables"
  ==> "Default"

RunTargetOrDefault "Default"
