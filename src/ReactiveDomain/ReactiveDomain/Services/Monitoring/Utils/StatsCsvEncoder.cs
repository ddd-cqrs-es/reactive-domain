﻿// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReactiveDomain.Services.Monitoring.Utils
{
    public static class StatsCsvEncoder
    {
        private const string Comma = ",";
        private const string CommaEscapeSymbol = ";";

        public static string GetHeader(Dictionary<string, object> stats)
        {
            return Join(stats.Keys).Prepend("Time");
        }

        public static string GetLine(Dictionary<string, object> stats)
        {
            return Join(stats.Values).PrependTime();
        }

        private static string Prepend(this string csvLine, string column)
        {
            return string.Format("{0}{1}{2}", column, Comma, csvLine);
        }

        private static string PrependTime(this string csvLine )
        {
            return csvLine.Prepend(DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        }

        private static string Join(IEnumerable<object> items )
        {
            var strValues = items.Select(TryGetInvariantString);
            var escapedValues = strValues.Select(str => str.Replace(Comma, CommaEscapeSymbol)); //extra safety
            
            return string.Join(Comma, escapedValues);
        }

        private static string TryGetInvariantString(object obj)
        {
            if (obj == null)
                return string.Empty;

            var convertible = obj as IConvertible;
            if (convertible != null)
                return convertible.ToString(CultureInfo.InvariantCulture);

            return obj.ToString();
        }
    }
}
