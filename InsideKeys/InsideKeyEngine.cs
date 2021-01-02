using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace InsideKeys
{
    public class InsideKeyEngine
        {
            private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            private readonly Dictionary<char, int> CharToIndex;

            //private readonly ThreadLocal<Random> Rands;
            
            [ThreadStatic]
            private static Random LocalRand;
        
            public InsideKeyEngine()
            {
                CharToIndex = new Dictionary<char, int>();
                
                //This is so that results wouldn't always be the same upon restart
                
                //Rands = new ThreadLocal<Random>(() => new Random((int)DateTime.UtcNow.Ticks));

                for (int I = 0; I <= 9; I++)
                {
                    CharToIndex.Add(Chars[I], I);
                }
            }
        
            [SkipLocalsInit]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe string GenCode(TimeSpan LifeTime)
            {
                //Memory is NOT Zero-ed
                
                var KeyNums = stackalloc int[13];

                var KeyChars = stackalloc char[24];
        
                const char Dash = '-';
        
                KeyChars[4] = Dash;
                
                KeyChars[9] = Dash;
                
                KeyChars[14] = Dash;
                
                KeyChars[19] = Dash;
                
                //Year-Month-Day-Seconds -> 2001-11-21-86400
            
                //Product Key -> XXXX-XXXX-2001-1121-86400

                var ExpiryDate = DateTime.UtcNow + LifeTime;
        
                //Year first since Year is guaranteed to be 2020 + ( We can't go back in time... )
                
                int Total = (ExpiryDate.Year * 1_0000) + (ExpiryDate.Month * 100) + (ExpiryDate.Day);
        
                //Generate first 8 letters

                var Rand = GetRand();
                
                int Offset = 0;
                
                for (int I = 0; I < 4; I++)
                {
                    char NewChar = Chars[Rand.Next(0, Chars.Length)];

                    KeyChars[I] = NewChar;

                    Offset += NewChar;
                }
                
                //Skip Dash
                
                for (int I = 5; I < 9; I++)
                {
                    char NewChar = Chars[Rand.Next(0, Chars.Length)];

                    KeyChars[I] = NewChar;

                    Offset += NewChar;
                }

                // //Offset MM/DD
                //
                // Total -= Offset;
                //
                // //Offset YYYY
                //
                // Total -= Offset * 1000;
                
                //Offset YYYY/MM/DD
                
                Total -= Offset * 1001;

                //Generate 8 chars based on Total; Total is guaranteed to be 8 digits
                //E.x. 2001-11-21, and max value of offset is below 1k

                SplitFast(Total, KeyNums, 8);

                //Generate remaining 5 digits based on Secs elapsed in the day
        
                //Ranges anywhere from 0 to 86400 - 1

                //int TotalSeconds = (int)DateTime.UtcNow.TimeOfDay.TotalSeconds; //Me being retarded

                //Since we are using SkipLocalsInit, we should zero fill the first 4 digits of the
                //last 5 digits!

                for (int I = 9; I <= 13; I++)
                {
                    KeyNums[I] = 0;
                }

                int TotalSeconds = (int)ExpiryDate.TimeOfDay.TotalSeconds;

                SplitFast(TotalSeconds, KeyNums, 13);

                int AccessCount = 0;

                //Skip first 9 chars since they are already generated ( Inclusive of dash )
                
                for (int I = 9; I < 25; I++)
                {
                    if (KeyChars[I] == '-')
                    {
                        continue;
                    }

                    KeyChars[I] = Chars[KeyNums[AccessCount]];

                    AccessCount++;
                }

                // for (int I = 0; I <= 25; I++)
                // {
                //     Console.WriteLine($"{I} | {KeyChars[I]}");
                // }

                return new string(KeyChars);
            }
        
            [SkipLocalsInit]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe DateTime Decode(string Key)
            {
                //Random----Time------Seconds
                
                //XXXX-XXXX-YYYY-YYYY-86400
                
                //Get ASCII representation of first 8 chars and sum them up...
            
                int Offset = 0;
            
                for (int I = 0; I < 4; I++)
                {
                    Offset += Key[I];
                }
                
                //Skip dash
                
                for (int I = 5; I < 9; I++)
                {
                    Offset += Key[I];
                }
                
                //Decode Time Chars into ints
                //Skip Dash
            
                var TimeInts = stackalloc int[8];
            
                int TimeIntsWriteCount = 0;

                for (int I = 10; I < 14; I++)
                {
                    //Guaranteed to return values 0-9
                    
                    TimeInts[TimeIntsWriteCount] = CharToIndex[Key[I]];

                    TimeIntsWriteCount++;
                }
                
                //Skip Dash
                
                for (int I = 15; I < 19; I++)
                {
                    //Guaranteed to return values 0-9
                    
                    TimeInts[TimeIntsWriteCount] = CharToIndex[Key[I]];

                    TimeIntsWriteCount++;
                }
                
                //We have to combine them to increment by offset

                var Total = CombineFast(TimeInts, 0, 8);

                // //Increment Total by Offset to get YYYY/MM/DD
                //
                // Total += Offset;
                
                //Console.WriteLine(Total);

                Total += Offset * 1001;
                
                //Increment Total by Offset to get YYYY/MM/DD
                
                //At this point, it looks like this -> 2001-11-21 ( Without the dashes of course )
                
                //We have to split them up again

                SplitFast(Total, TimeInts, 8);

                //Cache values of Year, Month and Day so we can reuse TimeInts array

                int Year = CombineFast(TimeInts, 0, 4);

                int Month = CombineFast(TimeInts, 4, 2);

                int Day = CombineFast(TimeInts, 6, 2);

                //Construct total elapsed seconds

                //Since we're reusing TimeInts, reset the WriteCount of it to 0

                TimeIntsWriteCount = 0;
                
                //Skip Dash

                for (int I = 20; I < 25; I++)
                {
                    TimeInts[TimeIntsWriteCount] = CharToIndex[Key[I]];
                    
                    TimeIntsWriteCount++;
                }
                
                //Now, construct Expiry DateTime

                int Seconds = CombineFast(TimeInts, 0, 5);

                //Console.WriteLine(Seconds);
                
                var DT = new DateTime(Year, Month, Day);

                return DT.AddSeconds(Seconds);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe int SplitFast(int Num, int* Output, int ArrayLength)
            {
                int I = 1;
                
                while (Num > 0)
                {
                    int Quotient = Num / 10;
                
                    int Remainder = Num - (Quotient * 10);
            
                    Num = Quotient;
        
                    Output[ArrayLength - I] = Remainder;

                    I++;
                }
        
                //+1 compensates for I++ exiting loop
                //Returns Starting Index
                
                return ArrayLength - I + 1;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe int CombineFast(int* Input, int StartingIndex, int ArrayLength)
            {
                int F = 0;

                //Decrement ArrayLength by 1 so that ArrayLength - Final value of I
                //Would be equal to 0, and anything powered by 0 yields 1 c;
                
                ArrayLength--;

                ArrayLength += StartingIndex;

                for (int I = StartingIndex; I <= ArrayLength; I++)
                {
                    F += Input[I] * PowFast(10, ArrayLength - I);
                }

                return F;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int PowFast(int Num, int Exp)
            {
                int result = 1;
                
                while (Exp > 0)
                {
                    if (Exp % 2 == 1)
                    {
                        result *= Num;
                    }
                    
                    Exp >>= 1;
                    
                    Num *= Num;
                }
            
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Random GetRand()
            {
                if (LocalRand == default)
                {
                    LocalRand = new Random((int) DateTime.UtcNow.Ticks);
                }

                return LocalRand;
            }
        }
}