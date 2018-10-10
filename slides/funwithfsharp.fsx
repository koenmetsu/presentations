(**
- title : Fun with F# while building my first OSS F# application
- description : Fun with F# while building my first OSS F# application
- author : koenmetsu
- theme : moon
- transition : slide

***

*)

(*** hide ***)
#I "../packages"
#r "../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../packages/Unquote/lib/net40/Unquote.dll"
open System

type Result<'success, 'failure> =
    | Success of 'success
    | Failure of 'failure

open FSharp.Data
(**


## Some cool stuff I liked in F# while building a pet project in my spare time
#### Koen Metsu

***

## What's my pet project about?

- Image archiver
- Uses ExifTool (perl) to extract EXIF|XMP|... data
- Archives based on date taken

[Muffin.Pictures.Archiver on GitHub](https://koenmetsu.github.io/Muffin.Pictures.Archiver/)

***
## What's F# about?

- Functional-first language
- Open source
- Cross-platform

***
## What's F# about?
- .NET Interoperability
- Immutable by default
- Strong type-system
- Concise yet robust
- Awesome community

***
# Now for the cool stuff
(that I actually used in my application)

***
## Easy to create wrapper types
Let's say you want a wrapper around a string in C#

```csharp
public class DirectoryPath
{
    private readonly string _path;

    public DirectoryPath(string path)
    {
        _path = path;
    }

    public string Path
    {
        get { return _path; }
    }

    public static implicit operator string(DirectoryPath directoryPath)
    {
        return directoryPath.Path;
    }
}
```

---
## Easy to create wrapper types
Let's say you want a wrapper around a string in F#
*)
type DirectoryPath = string

type FilePath = string

type TimeTaken = DateTimeOffset
(**
---
## Easy to create data types
(your plain old POCO)
*)
type MoveRequest = { Source : FilePath;
                     Destination : FilePath }

type FailedMove = { Request : MoveRequest;
                    Message : string }

type File = { FullPath:FilePath;
              Name:string }

(**
---
## Easy to extend types
*)

type Picture = { File:File;
                 TakenOn:TimeTaken } with
        member this.formatTakenOn =
            sprintf "%i-%02i"
                this.TakenOn.Year
                this.TakenOn.Month

(**
---
## Easy to create union types
(kind of like enums but way cooler)

*)
type Failure =
    | BytesDidNotMatch of MoveRequest
    | CouldNotCopyFile of FailedMove
    | CouldNotDeleteSource of FailedMove
    | OhDearGodItAllBlewUp
(**
***
## Type safety: pattern matching

*)
let formatFailure failure =
        match failure with
        | BytesDidNotMatch request ->
            "Reason: Bytes did not match"
        | CouldNotCopyFile move ->
            sprintf "Reason: Could not copy %A" move
        | CouldNotDeleteSource move ->
            "Reason: Could not delete source"
        | OhDearGodItAllBlewUp ->
            "You handle it from here"

(**
---
## Type safety: string formatting

*)
let report numberOfSuccesses author =
    sprintf "%i successes, go %s!"
        numberOfSuccesses
        author

let reported = report 1000000 "Koen"
(*** include-value: reported ***)
(**
---
## Type safety: inference

*)
let mapSomeOrNone f things =
    match things with
    | x::xs -> things |> Seq.map f |> Some
    | _ -> None

let mapped = mapSomeOrNone (fun x -> x + 20) [0 ; 5 ; 10]
let notMapped = mapSomeOrNone (fun x -> x + 20) []
(*** include-value: mapped ***)
(*** include-value: notMapped ***)
(**
***
## Easily pipeline operations
```
moveRequests
|> List.choose isSuccess
|> moveInParallel move
|> tee (fun _ -> watch.Stop())
|> createReport moveRequests
|> tee reportToConsole
|> reportToMailIfNecessary arguments
```

---
## Compose operations with ROP
```
let move moveWithFs compareFiles cleanUp =
    moveWithFs
    >=> compareFiles
    >=> cleanUp
```
![Railway-Oriented-Programming](images/funwithfsharp/Recipe_Railway_Transparent.png)

[source](http://fsharpforfunandprofit.com/rop/)

***
## F# Type providers
*)
open FSharp.Data

type Tags = JsonProvider<"example.json">
let tags = Tags.Load ("someFile.json")

let picture =
    tags
    |> Seq.find (fun pic -> pic.FileName = "bla")

let aperture = picture.Aperture
(**
[example json](https://raw.githubusercontent.com/koenmetsu/Muffin.Pictures.Archiver/master/src/Muffin.Pictures.Archiver/example_exiftool_output.json)

***
## .NET Interoperability
*)
open System.Diagnostics
let processStartInfo = new ProcessStartInfo()
processStartInfo.FileName <- "killdestroy.exe"
processStartInfo.Arguments <-
    sprintf "/r /y /c /b /a /r %i" 1234

Process.Start(processStartInfo)
(**
***
## Easy async
*)
let runAsync move =
    let asyncMove request =
        async { return move request }

    List.map asyncMove
    >> Async.Parallel
    >> Async.RunSynchronously
    >> List.ofArray
(**
***
## Functions as interfaces
*)
let compareFiles compare x y =
    if compare x y then
        sprintf
            "Wow! %s IS %s!" x y
    else
        "Awww..."

let badStrategy x y = x <> y
let areEqual = compareFiles badStrategy "Time" "Money"
(*** include-value: areEqual ***)
(**
***
## Easy stubs!
*)
open Swensen.Unquote

let ``Verify Awww... when things don't match up`` () =
    let stubComparer x y = false
    let comparison =
        compareFiles stubComparer "Time" "Money"

    test <@ "Awww..." = comparison @>

(*** hide ***)
let isOlderThanXMonths (currentTime: DateTimeOffset) months date =
        let currentTime = currentTime
        currentTime.AddMonths(-months) > date

(**
***
## Partial application
*)
let now = DateTimeOffset.UtcNow
let isOlderThan1Month = isOlderThanXMonths now 1
let firstOldDate =
    [ 0.0 .. 8.0 .. 100.0 ]
    |> Seq.map (fun i -> now.AddDays(-i))
    |> Seq.find isOlderThan1Month

(*** include-value: firstOldDate ***)
(**
***
### More information
[F# official site](http://fsharp.org/)

[F# For Fun and Profit](http://fsharpforfunandprofit.com)

![FSharp](images/funwithfsharp/fsharp.png)

[@koenmetsu](https://twitter.com/koenmetsu)
*)
