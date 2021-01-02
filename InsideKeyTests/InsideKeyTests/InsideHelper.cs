using System;

namespace CustomBot
{
    public unsafe ref struct StackStringBuilder
    {
        private readonly char* Arr;

        private int WriteIndex;

        public int Count => WriteIndex;
        
        #if DEBUG
        private int Size;
        #endif
        
        public StackStringBuilder(char* StackAllocChar, int size)
        {
            Arr = StackAllocChar;

            WriteIndex = 0;
            
            #if DEBUG
            Size = size;
            #endif
        }

        public void Append(char Item)
        {
            #if DEBUG
            if (WriteIndex >= Size)
            {
                throw new IndexOutOfRangeException(nameof(GetType));
            }
            #endif

            Arr[WriteIndex++] = Item;
        }
        
        public void Append(string Item)
        {
            #if DEBUG
            if (WriteIndex + Item.Length - 1 >= Size)
            {
                throw new IndexOutOfRangeException(nameof(GetType));
            }
            #endif

            for (int I = 0; I < Item.Length; I++)
            {
                Arr[WriteIndex++] = Item[I];
            }
        }

        public void RemoveChars(int count)
        {
            WriteIndex -= count;
            
            #if DEBUG
            if (WriteIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(GetType));
            }
            #endif
        }

        public new string ToString()
        {
            return new string(Arr);
        }
    }
    
    public static class InsideHelper
    {
        //Do NOT remove spacing prefixing the suffixes...!
            
        const string DaysSuffix = " Days(s)";
            
        const string HoursSuffix = " Hour(s)";
            
        const string MinsSuffix = " Min(s)";
            
        const string SecondsSuffix = " Sec(s)";
        
        public static unsafe string TimeSpanToWords(TimeSpan TS, string UndefinedMsg = "UNDEFINED!")
        {
            int Days = TS.Days;

            int Hours = TS.Hours;

            int Mins = TS.Minutes;

            int Seconds = TS.Seconds;

            int TotalSize = CalcTotalCharSize(Days, Hours, Mins, Seconds);
            
            var Alloc = stackalloc char[TotalSize];
            
            var SB = new StackStringBuilder(Alloc, TotalSize);
            
            if (Days != 0)
            {
                SB.Append($"{Days}{DaysSuffix}");
            }
            
            if (Hours != 0)
            {
                SB.Append($"{Hours}{HoursSuffix}");
            }
            
            if (Mins != 0)
            {
                SB.Append($"{Mins}{MinsSuffix}");
            }
            
            if (Seconds != 0)
            {
                SB.Append($"{Seconds}{SecondsSuffix}");
            }

            return (SB.Count != 0) ? SB.ToString() : UndefinedMsg;
        }

        private static int CalcTotalCharSize(int Days, int Hours, int Mins, int Seconds)
        {
            int I = 0;

            //Days

            I += CalcDigitsFast(Days) + DaysSuffix.Length;

            //Hours

            I += CalcDigitsFast(Hours) + HoursSuffix.Length;
            
            //Mins

            I += CalcDigitsFast(Mins) + MinsSuffix.Length;
            
            //Seconds

            I += CalcDigitsFast(Seconds) + SecondsSuffix.Length;

            return I;
        }

        private static int CalcDigitsFast(int Num)
        {
            if (Num == 0)
            {
                return 1;
            }
            
            int I = 0;

            while (Num > 0)
            {
                Num /= 10;

                I++;
            }

            return I;
        }
    }
}