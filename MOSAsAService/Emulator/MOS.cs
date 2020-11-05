using System.IO;
using Serilog;

namespace Emulator
{
    public class MOS
    {
        public readonly RAM Ram;
        public readonly CPU Cpu;
        private bool running = true;

        public MOS()
        {
            Ram = new RAM(RAM.RAM_SIZE);
            Cpu = new CPU
            {
                Ram = Ram
            };
            Ram.Cpu = Cpu;
            Cpu.PowerUp();
        }

        public void Run()
        {
            running = true;

            while (running)
            {
                Cpu.Instruction();
            }
        }

        public void Stop()
        {
            running = false;
        }

        /// <summary>
        /// Loads the ROM into memory, applying the mapper and copying the necessary bits to where they should go
        /// </summary>
        public void LoadROM()
        {
            
        }

    }
}
