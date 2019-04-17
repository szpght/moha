module ``Instruction decoder``
open Xunit
open FsUnit.Xunit
open Moha.Emulator.Moxie
open System

let instructionDecoder = new InstructionDecoder();

let forEachOpcodeOf opcodes test =
    let shiftedOpcodes =
        opcodes
        |> Seq.cast<int>
        |> Seq.map (fun x -> x <<< 8)
        |> Seq.cast<Opcode>
    for opcode in shiftedOpcodes do
        test opcode

let ``6-bit opcodes`` = [Opcode.Beq; Opcode.Bge; Opcode.Bgeu;
    Opcode.Bgt; Opcode.Bgtu; Opcode.Ble; Opcode.Bleu;
    Opcode.Blt; Opcode.Bltu; Opcode.Bne ]

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(511, 511)>]
[<InlineData(512, -512)>]
[<InlineData(513, -511)>]
[<InlineData(1023, -1)>]
let ``6-bit opcode: 10-bit parameter should be treated as signed`` (rawValue, expectedDecodedValue) =
    forEachOpcodeOf ``6-bit opcodes`` (fun opcode ->
        let encodedInstruction = uint16 opcode ||| uint16 rawValue
        let instruction = instructionDecoder.Decode <| encodedInstruction
        instruction.Value |> should equal expectedDecodedValue
    )

let ``4-bit opcodes`` =
    [ Opcode.Dec; Opcode.Gsr; Opcode.Inc; Opcode.Ssr ]

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(127, 127)>]
[<InlineData(255, 255)>]
let ``4-bit opcode: 8-bit parameter should be treated as unsigned`` (rawValue, expectedDecodedValue) =
    forEachOpcodeOf ``4-bit opcodes`` (fun opcode ->
        let encodedInstruction = uint16 opcode ||| uint16 rawValue
        let instruction = instructionDecoder.Decode encodedInstruction
        instruction.Value |> should equal expectedDecodedValue
    )

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(15, 15)>]
let ``4-bit opcode: register A encoded in bits 8-11`` (register, expectedDecodedValue) =
    forEachOpcodeOf ``4-bit opcodes`` (fun opcode ->
        let encodedInstruction = uint16 opcode ||| (uint16 register <<< 8)
        let instruction = instructionDecoder.Decode encodedInstruction
        instruction.RegisterA |> should equal register
    )

let ``8-bit opcodes`` =
    Enum.GetValues(typeof<Opcode>)
    |> Seq.cast<Opcode>
    |> Seq.except ``4-bit opcodes``
    |> Seq.except ``6-bit opcodes``

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(15, 15)>]
let ``8-bit opcode: register A encoded in bits 4-7`` (register, expectedDecodedValue) =
    forEachOpcodeOf ``8-bit opcodes`` (fun opcode ->
        let encodedInstruction = uint16 opcode ||| (uint16 register <<< 4)
        let instruction = instructionDecoder.Decode encodedInstruction
        instruction.RegisterA |> should equal register
    )

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(15, 15)>]
let ``8-bit opcode: register B encoded in bits 0-3`` (register, expectedDecodedValue) =
    forEachOpcodeOf ``8-bit opcodes`` (fun opcode ->
        let encodedInstruction = uint16 opcode ||| (uint16 register <<< 0)
        let instruction = instructionDecoder.Decode encodedInstruction
        instruction.RegisterB |> should equal register
    )