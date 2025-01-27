﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sieve.Models;
using Sieve.Services;
using SieveUnitTests.Entities;
using SieveUnitTests.Services;
using Xunit;

namespace SieveUnitTests
{
    public class StringFilterNullTests
    {
        private readonly IQueryable<Comment> _comments;
        private readonly SieveProcessor _processor;

        public StringFilterNullTests()
        {
            _processor = new SieveProcessor(new SieveOptionsAccessor());

            _comments = new List<Comment>
            {
                new Comment
                {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This text contains null somewhere in the middle of a string",
                    Author = "Dog",
                },
                new Comment
                {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "null is here twice in the text ending by null",
                    Author = "Cat",
                },
                new Comment
                {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "Regular comment without n*ll",
                    Author = "Mouse",
                },
                new Comment
                {
                    Id = 100,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = null,
                    Author = "null",
                },
                new Comment
                {
                    Id = 105,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "The duck wrote this",
                    Author = "Duck <5",
                }
            }.AsQueryable();
        }

        [Theory]
        [InlineData("Text==null")]
        [InlineData("Text==*null")]
        public void Filter_Equals_Null(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(100, result.Single().Id);
        }

        [Theory]
        [InlineData("Author==Duck <5")]
        [InlineData("Text|Author==Duck <5")]
        public void Filter_Equals_WithSpecialChar(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(105, result.Single().Id);
        }

        [Theory]
        [InlineData("Text!=null")]
        [InlineData("Text!=*null")]
        public void Filter_NotEquals_Null(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 0, 1, 2, 105 }, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text@=null")]
        [InlineData("Text@=*null")]
        [InlineData("Text@=*NULL")]
        [InlineData("Text@=*NulL")]
        [InlineData("Text@=*null|text")]
        public void Filter_Contains_NullString(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 0, 1 }, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text|Author==null", 100)]
        [InlineData("Text|Author@=null", 0, 1, 100)]
        [InlineData("Text|Author@=*null", 0, 1, 100)]
        [InlineData("Text|Author_=null", 1, 100)]
        [InlineData("Text|Author_=*null", 1, 100)]
        public void MultiFilter_Contains_NullString(string filter, params int[] expectedIds)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(expectedIds, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData(@"Author==\null", 100)]
        [InlineData(@"Author==*\null", 100)]
        [InlineData(@"Author==*\NuLl", 100)]
        [InlineData(@"Author!=*\null", 0, 1, 2, 105)]
        [InlineData(@"Author!=*\NulL", 0, 1, 2, 105)]
        [InlineData(@"Author!=\null", 0, 1, 2, 105)]
        public void SingleFilter_Equals_NullStringEscaped(string filter, params int[] expectedIds)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(expectedIds, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text_=null")]
        [InlineData("Text_=*null")]
        [InlineData("Text_=*NULL")]
        [InlineData("Text_=*NulL")]
        [InlineData("Text_=*null|text")]
        public void Filter_StartsWith_NullString(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 1 }, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text_-=null")]
        [InlineData("Text_-=*null")]
        [InlineData("Text_-=*NULL")]
        [InlineData("Text_-=*NulL")]
        [InlineData("Text_-=*null|text")]
        public void Filter_EndsWith_NullString(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 1 }, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text!@=null")]
        [InlineData("Text!@=*null")]
        [InlineData("Text!@=*NULL")]
        [InlineData("Text!@=*NulL")]
        [InlineData("Text!@=*null|text")]
        public void Filter_DoesNotContain_NullString(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 2, 105 }, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text!_=null")]
        [InlineData("Text!_=*null")]
        [InlineData("Text!_=*NULL")]
        [InlineData("Text!_=*NulL")]
        public void Filter_DoesNotStartsWith_NullString(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 0, 2, 105 }, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text!_-=null")]
        [InlineData("Text!_-=*null")]
        [InlineData("Text!_-=*NULL")]
        [InlineData("Text!_-=*NulL")]
        public void Filter_DoesNotEndsWith_NullString(string filter)
        {
            var model = new SieveModel { Filters = filter };

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] { 0, 2, 105 }, result.Select(p => p.Id));
        }
    }
}
