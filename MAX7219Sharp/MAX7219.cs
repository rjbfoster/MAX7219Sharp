using System.Device.Spi;
using System.Diagnostics;

namespace MAX7219Sharp
{
    public class MAX7219
    {
        /*
         * MAX7219 Register Address Map
         * No-Op        0x00
         * Digit 0      0x01
         * Digit 1      0x02
         * Digit 2      0x03
         * Digit 3      0x04
         * Digit 4      0x05
         * Digit 5      0x06
         * Digit 6      0x07
         * Digit 7      0x08
         * Decode Mode  0x09    Off: 0x00   On: 0x0F        When 'on', makes output easier but more restrictive
         * Intensity    0x0A    Min: 0x00   Max: 0x0F
         * Scan Limit   0x0B    All: 0x07
         * Shutdown     0x0C    On: 0x01    Off: 0x00
         * Display Test 0x0F
         */

        private readonly SpiDevice Device;
        public readonly Dictionary<char, byte> Characters;
        private static int DISPLAY_LENGTH;
        private readonly byte[] MODE_DECODE = { 0x09, 0x00 };        // Off
        private readonly byte[] MODE_INTENSITY = { 0x0A, 0x00 };     // Minimum
        private readonly byte[] MODE_SCAN_LIMIT = { 0x0B, 0x07 };    // All 8 digits
        private readonly byte[] MODE_POWER = { 0x0C, 0x01 };         // Display on
        private CancellationTokenSource cancellationTokenSource = new();
        private Task? scrollingTask = null;

        public MAX7219()
        {
            // Default constructor: Uses SPI0, CE0, 1MHz, Mode0, on an 8 digit display.
            try
            {
                var settings = new SpiConnectionSettings(0, 0)
                {
                    ClockFrequency = 1000000,   // Set the SPI bus speed to 1 MHz
                    Mode = SpiMode.Mode0        // Set the SPI mode to Mode0
                };
                Device = SpiDevice.Create(settings);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to initialize SPI device: {e.Message}.");
                throw;
            }
            Device.Write(MODE_DECODE);
            Device.Write(MODE_INTENSITY);
            Device.Write(MODE_SCAN_LIMIT);
            Device.Write(MODE_POWER);
            DISPLAY_LENGTH = 8;
            Characters = InitialiseValues();
        }

        public MAX7219(int DisplayLength)
        {
            // Default constructor: Uses SPI0, CE0, 1MHz, Mode0.
            try
            {
                var settings = new SpiConnectionSettings(0, 0)
                {
                    ClockFrequency = 1000000, // Set the SPI bus speed to 1 MHz
                    Mode = SpiMode.Mode0 // Set the SPI mode to Mode0
                };
                Device = SpiDevice.Create(settings);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to initialize SPI device: {e.Message}.");
                throw;
            }
            Device.Write(MODE_DECODE);
            Device.Write(MODE_INTENSITY);
            Device.Write(MODE_SCAN_LIMIT);
            Device.Write(MODE_POWER);
            DISPLAY_LENGTH = DisplayLength;
            Characters = InitialiseValues();
        }

        public MAX7219(SpiDevice device)
        {
            // Constructor: Uses user-supplied SpiDevice.
            Device = device;
            Device.Write(MODE_DECODE);
            Device.Write(MODE_INTENSITY);
            Device.Write(MODE_SCAN_LIMIT);
            Device.Write(MODE_POWER);
            DISPLAY_LENGTH = 8;
            Characters = InitialiseValues();
        }

        public MAX7219(SpiDevice device, int DisplayLength)
        {
            // Constructor: Uses user-supplied SpiDevice.
            Device = device;
            Device.Write(MODE_DECODE);
            Device.Write(MODE_INTENSITY);
            Device.Write(MODE_SCAN_LIMIT);
            Device.Write(MODE_POWER);
            DISPLAY_LENGTH = DisplayLength;
            Characters = InitialiseValues();
        }

        public void SetDefault(Mode mode)
        {
            // Sets chosen mode to default values
            switch (mode)
            {
                case Mode.Decode:
                    Device.Write(MODE_DECODE);
                    break;
                case Mode.Intensity:
                    Device.Write(MODE_INTENSITY);
                    break;
                case Mode.ScanLimit:
                    Device.Write(MODE_SCAN_LIMIT);
                    break;
                case Mode.Power:
                    Device.Write(MODE_POWER);
                    break;
            }
        }

        public void SetDecode(Decode value)
        {
            switch (value)
            {
                case Decode.On:
                    Device.Write(new byte[] { 0x09, 0x0F });
                    break;
                case Decode.Off:
                    Device.Write(new byte[] { 0x09, 0x00 });
                    break;
            }
        }

        public void SetManual(Mode mode, byte[] value)
        {
            // Sets chosen modes to user-supplied value
            switch (mode)
            {
                case Mode.Decode:
                    Device.Write(new byte[] { 0x09, value[0] });
                    break;
                case Mode.Intensity:
                    Device.Write(new byte[] { 0x0A, value[0] });
                    break;
                case Mode.ScanLimit:
                    Device.Write(new byte[] { 0x0B, value[0] });
                    break;
                case Mode.Power:
                    Device.Write(new byte[] { 0x0C, value[0] });
                    break;
            }
        }

        public void SetPower(Power value)
        {
            switch (value)
            {
                case Power.On:
                    Device.Write(new byte[] { 0x0C, 0x01 });
                    break;
                case Power.Off:
                    Device.Write(new byte[] { 0x0C, 0x00 });
                    break;
            }
        }
        public enum Mode
        {
            Decode,
            Intensity,
            ScanLimit,
            Power
        }

        public enum Decode
        {
            On,
            Off
        }
        public enum Power
        {
            On,
            Off
        }
        public void AllOn()
        {
            // Turn all LEDs on
            Device.Write(new byte[] { 0x0F, 0xFF });
        }
        public void AllOff()
        {
            // Turn all LEDs off
            Device.Write(new byte[] { 0x0F, 0x00 });
        }
        public void ClearDisplay()
        {
            // Clear the display
            for (int i = 1; i <= 8; i++)
            {
                byte[] clearDisplay = new byte[2];
                clearDisplay[0] = (byte)i;
                clearDisplay[1] = 0x00;
                Device.Write(clearDisplay);
            }
        }
        public void Scroll(string scrollMessage, int durationSeconds = 60)
        {
            // Scrolls a message from right to left on the display

            // Cancel the previous task if it exists
            if (scrollingTask?.IsCompleted == false)
            {
                cancellationTokenSource.Cancel();
            }

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            scrollingTask = ScrollMessage(scrollMessage, durationSeconds, cancellationToken);
        }
        private async Task ScrollMessage(string message, int duration, CancellationToken cancellationToken)
        {
            // Private handler for the 'Scroll()' method above
            int messageLength = message.Length;

            // Add spaces before and after the message to create a scrolling effect
            message = $"  {message}  ";

            // If message is less than 8 characters, pad it out
            if (messageLength < DISPLAY_LENGTH)
            {
                message = message.PadRight(DISPLAY_LENGTH);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            TimeSpan runFor = TimeSpan.FromSeconds(duration);
            int index = 0;
            while (stopwatch.Elapsed < runFor)
            {
                foreach (char c in message)
                {
                    if (c == '.')
                    {
                        continue;
                    }
                    if (Characters.TryGetValue(c, out byte value))
                    {
                        if (index < messageLength && message[index + 1] == '.')
                        {
                            value |= 0b10000000; // Set DP register to show decimal point
                        }
                        byte registerIndex = (byte)(DISPLAY_LENGTH - Math.Min(index, DISPLAY_LENGTH));
                        byte[] output = new byte[] { registerIndex, value };
                        Device.Write(output);
                        index++;
                    }
                }

                // Shift the message string to the left by one character
                message = message[1..] + message[0];

                // Sleep for a short time to control the scrolling speed
                await Task.Delay(100, cancellationToken);
                index = 0;
            }

            // Clear the display after the scrolling has finished
            ClearDisplay();
        }
        public void WriteMessage(string inputMessage)
        {
            // Write the input to the LED display
            ClearDisplay();
            string inputNoDecimal = inputMessage.Replace(".", "");
            if (string.IsNullOrEmpty(inputMessage))
            {
                Console.WriteLine("Invalid input");
                return;
            }
            if (inputNoDecimal.Length > DISPLAY_LENGTH)
            {
                // If the input is too long, shorten it
                int buffer = 0;
                int totalchars = 0;
                foreach (var letter in inputNoDecimal)
                {
                    if (letter == '.')
                    {
                        buffer++;
                    }
                    else
                    {
                        totalchars++;
                    }
                    if (totalchars >= DISPLAY_LENGTH)
                    {
                        break;
                    }
                }
                inputMessage = inputMessage[..(DISPLAY_LENGTH + buffer)];
            }

            byte[] output = new byte[2];
            int index = 0;
            foreach (char c in inputMessage)
            {
                if (c == '.')
                {
                    continue;
                }
                if (!Characters.TryGetValue(c, out byte value))
                {
                    continue;
                }

                if (index < DISPLAY_LENGTH)
                {
                    if (index < inputMessage.Length - 1 && inputMessage[index + 1] == '.')
                    {
                        value |= 0b10000000; // Set DP register to show decimal point
                    }
                    output[0] = (byte)(DISPLAY_LENGTH - index);
                    output[1] = value;
                    Device.Write(output);
                    index++;
                }
                else
                {
                    break;
                }
            }
        }
        private static Dictionary<char, byte> InitialiseValues()
        {
            var chars = new Dictionary<char, byte>
            {
                {'A',0b1110111},
                {'B',0b1111111},
                {'C',0b1001110},
                {'D',0b1111110},
                {'E',0b1001111},
                {'F',0b1000111},
                {'G',0b1011110},
                {'H',0b0110111},
                {'I',0b0110000},
                {'J',0b0111100},
                {'K',0b0110001},
                {'L',0b0001110},
                {'M',0b0010101},
                {'N',0b1110110},
                {'O',0b1111110},
                {'P',0b1100111},
                {'Q',0b1110011},
                {'R',0b0000101},
                {'S',0b1011011},
                {'T',0b0001111},
                {'U',0b0111110},
                {'V',0b0111010},
                {'W',0b0101010},
                {'X',0b0100101},
                {'Y',0b0100111},
                {'Z',0b1101101},
                {'[',0b1001110},
                {']',0b1111000},
                {'_',0b0001000},
                {'a',0b1110111},
                {'b',0b0011111},
                {'c',0b0001101},
                {'d',0b0111101},
                {'e',0b1001111},
                {'f',0b1000111},
                {'g',0b1011110},
                {'h',0b0010111},
                {'i',0b0010000},
                {'j',0b0111100},
                {'k',0b0110001},
                {'l',0b0001110},
                {'m',0b0010101},
                {'n',0b0010101},
                {'o',0b1111110},
                {'p',0b1100111},
                {'q',0b1110011},
                {'r',0b0000101},
                {'s',0b1011011},
                {'t',0b0001111},
                {'u',0b0011100},
                {'v',0b0011100},
                {'w',0b0101010},
                {'x',0b0100101},
                {'y',0b0100111},
                {'z',0b1101101},
                {'-',0b0000001},
                {':',0b0000001}, // Same as -
                {' ',0b0000000},
                {'0',0b1111110},
                {'1',0b0110000},
                {'2',0b1101101},
                {'3',0b1111001},
                {'4',0b0110011},
                {'5',0b1011011},
                {'6',0b1011111},
                {'7',0b1110000},
                {'8',0b1111111},
                {'9',0b1111011}
            };
            return chars;
        }
    }
}