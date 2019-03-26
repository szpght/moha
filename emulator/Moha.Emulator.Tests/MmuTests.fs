module MmuTests
open Xunit
open FsUnit.Xunit
open Moha.Emulator.Moxie
open System
open Swensen.Unquote

let memorySize = 512
let destinationOffset = memorySize / 2 |> uint32
let sampleData =
    [0xFF; 0xFF; 0xFF; 0xDE; 0xAD; 0xBE; 0xEF; 0xFF]
    |> Seq.map (fun x -> byte x)
    |> Seq.toArray
    |> ReadOnlyMemory<byte>
let createMmu () = 
    let mmu = new Mmu(memorySize)
    mmu.CopyToPhysical(destinationOffset, sampleData.Span)    
    mmu
let withMmu test = createMmu() |> test
let getAddress (offset: int) = uint32 (destinationOffset + uint32 offset)

[<Fact>]
let ``GetByte: works`` () =
    withMmu (fun mmu ->
        let getByte (offset: int) = int (mmu.GetByte (getAddress offset))
        
        test <@ getByte 3 = 0xDE @>
        test <@ getByte 4 = 0xAD @>
        test <@ getByte 5 = 0xBE @>
        test <@ getByte 6 = 0xEF @>
    )

[<Fact>]
let ``GetShort: works for unaligned access`` () =
    withMmu (fun mmu ->
        let getShort (offset: int) = int (mmu.GetShort (getAddress offset))
        test <@ getShort 3 = 0xADDE @>
    )

[<Fact>]
let ``GetShort: works for aligned access`` () =
    withMmu (fun mmu ->
        let getShort (offset: int) = int (mmu.GetShort (getAddress offset))
        test <@ getShort 4 = 0xBEAD @>
    )

[<Fact>]
let ``GetShort: throws when range exceeded by 1 byte`` () =
    withMmu (fun mmu ->
        let getOutOfRangeWord () = mmu.GetShort (uint32 (memorySize - 1)) |> ignore
        getOutOfRangeWord |> should throw typeof<IndexOutOfRangeException>
    )