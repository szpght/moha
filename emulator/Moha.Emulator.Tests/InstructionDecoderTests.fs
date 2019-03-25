module InstructionDecoderTests
open Xunit
open FsUnit.Xunit
open Moha.Emulator.Moxie

let instructionDecoder = new InstructionDecoder();

let ``6-bit opcodes`` = [Opcode.Beq; Opcode.Bge; Opcode.Bgeu;
    Opcode.Bgt; Opcode.Bgtu; Opcode.Ble; Opcode.Bleu;
    Opcode.Blt; Opcode.Bltu; Opcode.Bne ]

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(511, 511)>]
[<InlineData(512, -512)>]
[<InlineData(513, -511)>]
[<InlineData(1023, -1)>]
let ``6-bit opcode: 10-bit parameter should be treated as signed`` (parameterToEncode, expectedValue) =
    let encodedInstructions =
        ``6-bit opcodes``
        |> List.map (fun opcode -> uint16 opcode ||| uint16 parameterToEncode)

    let decodedValues =
        encodedInstructions
        |> List.map instructionDecoder.Decode
        |> List.map (fun instr -> instr.Value)
    
    for value in decodedValues do
        value |> should equal expectedValue
