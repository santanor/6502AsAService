using System;
using Emulator.Utils;

namespace Emulator
{
    public class RAM
    {
        public const int RAM_SIZE = 65536;

        private byte[] bank;

        public CPU Cpu;

        public RAM(int size)
        {
            bank = new byte[size];
        }

        /// <summary>
        /// Reads a byte from the specified memory location
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte Byte(ushort address)
        {
            return bank[address];
        }

        public byte Byte(int address)
        {
            return Byte((ushort)address);
        }

        /// <summary>
        /// Reads two bytes from the starting position and returns it as a memory address.
        /// It will do the calculation for you, that's how nice this bad boy is
        /// </summary>
        public ushort Word(ushort address)
        {
            var result = new byte[2];
            result[0] = Byte(address);
            result[1] = Byte((ushort)(address + 1));

            return result.ToWord();
        }

        public ushort Word(int address)
        {
            return Word((ushort)address);
        }

        /// <summary>
        /// Writes a byte to the specified memory location
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void WriteByte(ushort addr, byte value)
        {
            bank[addr] = value;
        }

        /// <summary>
        /// Writes a word to the specified memory location, the word
        /// is internally converted and flipped to be inserted into the memory bank
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void WriteWord(ushort addr, ushort value)
        {
            var bytes = value.ToBytes();
            for (var i = 0; i < bytes.Length; i++)
            {
                bank[addr + i] = bytes[i];
            }
        }


        /// <summary>
        /// Push a byte on top of the stack
        /// </summary>
        /// <param name="value"></param>
        public void PushByte(byte value)
        {
            var bankPointer = (ushort)(Cpu.SP + 0x100);
            WriteByte(bankPointer, value);
            Cpu.SP -= 1;
        }

        /// <summary>
        /// Push a word on top of the stack, internally the word is flipped to reflect
        /// the correct endian-ess
        /// </summary>
        /// <param name="value"></param>
        public void PushWord(ushort value)
        {
            var bankPointer = (ushort)(Cpu.SP + 0xFF); // 100 - 1 but in hex -> 99 is 0xFF;
            WriteWord(bankPointer, value);
            Cpu.SP -= 2;
        }

        /// <summary>
        /// Pulls the top-most byte from the stack
        /// </summary>
        /// <returns></returns>
        public byte PopByte()
        {
            var value = Byte((ushort)(Cpu.SP + 1 + 0x100));
            Cpu.SP += 1;
            return value;
        }

        /// <summary>
        /// Pops the 2 top-most bytes from the stack and returns them as a word
        /// </summary>
        /// <returns></returns>

        public ushort PopWord()
        {
            var value = Word((ushort)(Cpu.SP + 1 + 0x100));
            Cpu.SP += 2;
            return value;
        }

        /// <summary>
        /// Clears the contents of the bank, setting them to 0
        /// </summary>
        public void Zero()
        {
            Array.Fill<byte>(bank, 0);
        }

        /// <summary>
        /// Mockup method, simply returns what its given. It's here for consistency
        /// </summary>
        public ushort Absolute(ushort addr)
        {
            return addr;
        }

        /// <summary>
        /// Absolute X
        /// Returns whatever is entered as a parameter plus register X
        /// </summary>
        public ushort AbsoluteX(ushort addr, bool checkIfPageCrossed = false)
        {
            if (checkIfPageCrossed)
            {
                if ((addr & 0xff00) != ((addr + Cpu.X) & 0xff00)) {
                    Cpu.cyclesThisSec++;
                }
            }
            return (ushort)(addr + Cpu.X);
        }

        /// <summary>
        /// Absolute Y
        /// Returns whatever is entered as a parameter plus register Y
        /// </summary>
        public ushort AbsoluteY(ushort addr)
        {
            return (ushort)(addr + Cpu.Y);
        }

        public ushort ZPage(ushort addr)
        {
            return (ushort)(addr & 0x00FF);
        }

        /// <summary>
        /// Zero Page X
        /// Gets the content of the parameter, adds X to it and that points
        /// to an address in the zero page where the parameter can be found
        /// </summary>
        public ushort ZPageX(byte addr)
        {
            return (ushort)((addr + Cpu.X) & 0x00FF);
        }

        /// <summary>
        /// Zero Page Y
        /// Gets the content of the parameter, adds X to it and that points
        /// to an address in the zero page where the parameter can be found
        /// </summary>
        public ushort ZPageY(byte addr)
        {
            return (ushort)((addr + Cpu.Y) & 0x00FF);
        }

        /// <summary>
        /// Basically the way it works, it gets the value of the parameter, adds the register X to it. That is an
        /// address from which we get one byte, shift it left by 8 bits, read the next position and add t hat to the
        /// shifted value. THAT is the indirectX address of it
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public ushort IndirectX(byte addr)
        {
            addr = (byte)ZPageX(addr);
            //Read a byte from addr and a byte from addr+1. But in both cases wraparound so hence the 0xFF mask
            return (ushort)(Byte(addr & 0xFF) | Byte((addr + 1) & 0xFF) << 8);
        }

        public ushort IndirectY(byte addr, bool checkPageCrossed = false)
        {
            var result =(ushort)(Byte(addr & 0xFF) | Byte((addr + 1) & 0xFF) << 8);
            
            if (checkPageCrossed)
            {
                if ((result & 0xff00) != ((result + Cpu.Y) & 0xff00)) {
                    Cpu.cyclesThisSec++;
                }
                
            }
            return (ushort)(result + Cpu.Y);
        }

        public byte ZPageXParam() => Byte(ZPageX(Byte(Cpu.PC + 1)));
        public byte ZPageYParam() => Byte(ZPageY(Byte(Cpu.PC + 1)));
        public byte ZPageParam() => Byte(Byte(Cpu.PC + 1) & 0x00FF);
        public byte AbsoluteParam() => Byte(Absolute(Word(Cpu.PC + 1)));

        public byte AbsoluteXParam(bool checkPageCrossed = false)
        {
            var parameter = Word(Cpu.PC + 1);
            var absX = AbsoluteX(parameter, checkPageCrossed);
            var result = Byte(absX);

            return result;
        }

        public byte AbsoluteYParam(bool checkPageCrossed = false)
        {
            var parameter = Word(Cpu.PC + 1);
            var absY = AbsoluteY(parameter);
            var result = Byte(absY);

            if (checkPageCrossed)
            {
                CheckPageCrossed(parameter, absY);
            }

            return result;
        }

        public byte IndirectXParam() => Byte(IndirectX(Byte(Cpu.PC + 1)));

        public byte IndirectYParam(bool checkPageCrossed = false)
        {
            var parameter = Byte(Cpu.PC + 1);
            var indY = IndirectY(parameter, checkPageCrossed);
            var result = Byte(indY);

            return result;
        }

        public ushort IndirectParam()
        {
            ushort addr = Word(Cpu.PC + 1);
            ushort targetAddr = 0x0000;
            // This is a 6502 bug when instead of reading from $C0FF/$C100 it reads from $C0FF/$C000
            if ((addr & 0xFF) == 0xFF) {
                // Buggy code
                targetAddr = (ushort) ((Byte(addr & 0xFF00) << 8) + Byte(addr));
            } else {
                // Normal code
                targetAddr = Word(addr);
            }

            return targetAddr;
        }


        public void CheckPageCrossed(ushort addr1, ushort addr2)
        {
            if ((addr1 & 0xFF00) != (addr2 & 0xFF00))
            {
                Cpu.cyclesThisSec++;
            }
        }
    }
}
