﻿using System;
using System.Linq;
using Jomlet.Models;
using Xunit;

namespace Jomlet.Tests
{
    public class DateTimeTests
    {
        private JomlDocument GetDocument(string resource)
        {
            var parser = new JomlParser();
            return parser.Parse(resource);
        }

        [Fact]
        public void OffsetDateTimesWork()
        {
            var document = GetDocument(TestResources.OffsetDateTimeTestInput);

            Assert.Equal(4, document.Entries.Count);

            var targetDateTime = new DateTime(1979, 5, 27, 07, 32, 00, DateTimeKind.Utc);
            foreach (var (key, value) in document.Entries)
            {
                if (key == "odt3")
                    //odt3 has .999999 on the end.
                    Assert.Equal(targetDateTime.AddTicks(9999990), Assert.IsType<JomlOffsetDateTime>(value).Value.ToUniversalTime());
                else
                    Assert.Equal(targetDateTime, Assert.IsType<JomlOffsetDateTime>(value).Value.ToUniversalTime());
            }
        }

        [Fact]
        public void LocalDateTimesWork()
        {
            var document = GetDocument(TestResources.LocalDateTimeTestInput);

            Assert.Equal(2, document.Entries.Count);

            //Check ldt1
            Assert.Equal(
                new DateTime(1979, 5, 27, 07, 32, 00, DateTimeKind.Unspecified),
                Assert.IsType<JomlLocalDateTime>(document.Entries.Values.First()).Value
            );

            //Check ldt2
            Assert.Equal(
                new DateTime(1979, 5, 27, 0, 32, 00, DateTimeKind.Unspecified).AddTicks(9999990),
                Assert.IsType<JomlLocalDateTime>(document.Entries.Values.Last()).Value
            );
        }

        [Fact]
        public void LocalDatesWork()
        {
            var document = GetDocument(TestResources.LocalDateTestInput);

            Assert.Single(
                document.Entries.Values,
                value => Assert.IsType<JomlLocalDate>(value).Value == new DateTime(1979, 5, 27)
            );
        }

        [Fact]
        public void LocalTimesWork()
        {
            var document = GetDocument(TestResources.LocalTimeTestInput);
            
            Assert.Equal(2, document.Entries.Count);
            
            //Check lt1
            Assert.Equal(
                new TimeSpan(7, 32, 00),
                Assert.IsType<JomlLocalTime>(document.Entries.Values.First()).Value
            );

            //Check lt2
            Assert.Equal(
                new TimeSpan(0, 32, 00).Add(TimeSpan.FromTicks(9999990)),
                Assert.IsType<JomlLocalTime>(document.Entries.Values.Last()).Value
            );
        }
    }
}