module ``MMU``
open Xunit
open FsUnit.Xunit
open Moha.Emulator.Moxie
open System
open Swensen.Unquote

let memorySize = 512
let destinationOffset = memorySize / 2 |> uint32
let destinationIndex = (int destinationOffset) / 2
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
let withOffsetRelativeToSampleData (offset: int) getter = getter <| uint32 (destinationOffset + (uint32 offset))
let withAbsoluteAddress (offset: int) getter = getter <| uint32 offset

[<Fact>]
let ``GetByte: works`` () =
    withMmu (fun mmu ->
        let getByte offset =
            mmu.GetByte
            |> withOffsetRelativeToSampleData offset

        test <@ getByte 3 = 0xDEuy @>
        test <@ getByte 4 = 0xADuy @>
        test <@ getByte 5 = 0xBEuy @>
        test <@ getByte 6 = 0xEFuy @>
    )

[<Fact>]
let ``GetShort: works for unaligned access`` () =
    withMmu (fun mmu ->
        mmu.GetShort
        |> withOffsetRelativeToSampleData 3
        |> should equal 0xADDEus
    )

[<Fact>]
let ``GetShort: works for aligned access`` () =
    withMmu (fun mmu ->
        mmu.GetShort
        |> withOffsetRelativeToSampleData 4
        |> should equal 0xBEADus
    )

[<Fact>]
let ``GetShort: throws when range exceeded by 1 byte`` () =
    withMmu (fun mmu ->
        let getOutOfRangeWord () =
            mmu.GetShort
            |> withAbsoluteAddress (memorySize - 1)
            |> ignore
        getOutOfRangeWord |> should throw typeof<IndexOutOfRangeException>
    )

[<Fact>]
let ``GetLong: works for unaligned access`` () =
    withMmu (fun mmu ->
        mmu.GetLong
        |> withOffsetRelativeToSampleData 3
        |> should equal 0xEFBEADDEu
    )

[<Fact>]
let ``GetLong: works for aligned access`` () =
    withMmu (fun mmu ->
        mmu.GetLong
        |> withOffsetRelativeToSampleData 4
        |> should equal 0xFFEFBEADu
    )
    
[<Fact>]
let ``GetLong: throws when range exceeded by 1 byte`` () =
    withMmu (fun mmu ->
        let getOutOfRangeLong () =
            mmu.GetLong
            |> withAbsoluteAddress (memorySize - 3)
            |> ignore
        getOutOfRangeLong |> should throw typeof<IndexOutOfRangeException>
    )

let exampleStoreAddress = uint32 259

[<Fact>]
let ``StoreByte: stored value can be retrieved`` () =
    withMmu (fun mmu ->
        mmu.StoreByte(exampleStoreAddress, 0xCAuy)
        mmu.StoreByte(exampleStoreAddress + (uint32 1), 0xFEuy)

        mmu.GetByte(exampleStoreAddress) |> should equal 0xCAuy
        mmu.GetByte(exampleStoreAddress + (uint32 1)) |> should equal 0xFEuy
    )

[<Fact>]
let ``StoreShort: stored value can be retrieved`` () =
    withMmu (fun mmu ->
        mmu.StoreShort(exampleStoreAddress, 0xBABEus)
        mmu.GetShort(exampleStoreAddress) |> should equal 0xBABEus
    )

[<Fact>]
let ``StoreLong: stored value can be retrieved`` () =
    withMmu (fun mmu ->
        mmu.StoreLong(exampleStoreAddress, 0xDEADBAADul)
        mmu.GetLong(exampleStoreAddress) |> should equal 0xDEADBAADul
    )
