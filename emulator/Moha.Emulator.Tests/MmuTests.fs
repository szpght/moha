module MmuTests
open Xunit
open FsUnit.Xunit
open Moha.Emulator.Moxie
open System
open Swensen.Unquote

let memorySize = 512
let createMmu () = new Mmu(memorySize)
let withMmu test = createMmu() |> test
let destinationOffset = memorySize / 2 |> uint32
let sampleData =
    [0xFF; 0xFF; 0xFF; 0xDE; 0xAD; 0xBE; 0xEF; 0xFF]
    |> Seq.map (fun x -> byte x)
    |> Seq.toArray
    |> ReadOnlyMemory<byte>
    
[<Fact>]
let ``GetByte: works`` () =
    withMmu (fun mmu ->
        mmu.CopyToPhysical(destinationOffset, sampleData.Span)
        let getByte (offset: int) = int (mmu.GetByte (uint32 (destinationOffset + uint32 offset)))
        
        test <@ getByte 2 = 0xFF @>
        test <@ getByte 3 = 0xDE @>
        test <@ getByte 4 = 0xAD @>
        test <@ getByte 5 = 0xBE @>
        test <@ getByte 6 = 0xEF @>
        test <@ getByte 7 = 0xFF @>
    )

