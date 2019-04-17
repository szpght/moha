module ``Virtual memory``
open Xunit
open FsUnit.Xunit
open Moha.Emulator.Moxie
open System
open Swensen.Unquote

let memorySize = 8192
let virtualOffset = uint32 4096
let sampleData =
    [0xFF; 0xFF; 0xFF; 0xDE; 0xAD; 0xBE; 0xEF; 0xFF]
    |> Seq.map (fun x -> byte x)
    |> Seq.toArray
    |> ReadOnlyMemory<byte>
let createMmu () = 
    let mmu = new Mmu(memorySize)
    mmu.CopyToPhysical(uint32 0, sampleData.Span)    
    mmu

[<Fact>]
let ``Read byte from TLB`` () =
    let mmu = createMmu()
    let byte = mmu.GetByte(virtualOffset + uint32 3)
    byte |> should equal 0xDEuy
