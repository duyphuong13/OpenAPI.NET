﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Properties;
using Microsoft.OpenApi.Services;
using Microsoft.OpenApi.Validations.Rules;
using Xunit;

namespace Microsoft.OpenApi.Validations.Tests
{
    [Collection("DefaultSettings")]
    public class OpenApiSchemaValidationTests
    {
        [Fact]
        public void ValidateDefaultShouldNotHaveDataTypeForSimpleSchemaMismatch()
        {
            // Arrange
            IEnumerable<OpenApiError> errors;
            var schema = new OpenApiSchema()
            {
                Default = new OpenApiInteger(55),
                Type = "string",
            };

            // Act
            var validator = new OpenApiValidator(ValidationRuleSet.GetDefaultRuleSet());
            var walker = new OpenApiWalker(validator);
            walker.Walk(schema);

            errors = validator.Errors;
            bool result = !errors.Any();

            // Assert
            result.Should().BeFalse();
            errors.Select(e => e.Message).Should().BeEquivalentTo(new[]
            {
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage
            });
            errors.Select(e => e.Pointer).Should().BeEquivalentTo(new[]
            {
                "#/default",
            });
        }

        [Fact]
        public void ValidateExampleAndDefaultShouldNotHaveDataTypeForSimpleSchemaMismatch()
        {
            // Arrange
            IEnumerable<OpenApiError> errors;
            var schema = new OpenApiSchema()
            {
                Example = new OpenApiLong(55),
                Default = new OpenApiPassword("1234"),
                Type = "string",
            };

            // Act
            var validator = new OpenApiValidator(ValidationRuleSet.GetDefaultRuleSet());
            var walker = new OpenApiWalker(validator);
            walker.Walk(schema);

            errors = validator.Errors;
            bool result = !errors.Any();

            // Assert
            result.Should().BeFalse();
            errors.Select(e => e.Message).Should().BeEquivalentTo(new[]
            {
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage
            });
            errors.Select(e => e.Pointer).Should().BeEquivalentTo(new[]
            {
                "#/default",
                "#/example",
            });
        }

        [Fact]
        public void ValidateEnumShouldNotHaveDataTypeForSimpleSchemaMismatch()
        {
            // Arrange
            IEnumerable<OpenApiError> errors;
            var schema = new OpenApiSchema()
            {
                Enum =
                {
                    new OpenApiString("1"),
                    new OpenApiObject()
                    {
                        ["x"] = new OpenApiInteger(2),
                        ["y"] = new OpenApiString("20"),
                        ["z"] = new OpenApiString("200")
                    },
                    new OpenApiArray()
                    {
                        new OpenApiInteger(3)
                    },
                    new OpenApiObject()
                    {
                        ["x"] = new OpenApiInteger(4),
                        ["y"] = new OpenApiInteger(40),
                    },
                },
                Type = "object",
                AdditionalProperties = new OpenApiSchema()
                {
                    Type = "integer",
                }
            };

            // Act
            var validator = new OpenApiValidator(ValidationRuleSet.GetDefaultRuleSet());
            var walker = new OpenApiWalker(validator);
            walker.Walk(schema);

            errors = validator.Errors;
            bool result = !errors.Any();

            // Assert
            result.Should().BeFalse();
            errors.Select(e => e.Message).Should().BeEquivalentTo(new[]
            {
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
            });
            errors.Select(e => e.Pointer).Should().BeEquivalentTo(new[]
            {
                "#/enum/0",
                "#/enum/1/y",
                "#/enum/1/z",
                "#/enum/2"
            });
        }

        [Fact]
        public void ValidateDefaultShouldNotHaveDataTypeForComplexSchemaMismatch()
        {
            // Arrange
            IEnumerable<OpenApiError> errors;
            var schema = new OpenApiSchema()
            {
                Type = "object",
                Properties =
                {
                    ["property1"] = new OpenApiSchema()
                    {
                        Type = "array",
                        Items = new OpenApiSchema()
                        {
                            Type = "integer",
                            Format = "int64"
                        }
                    },
                    ["property2"] = new OpenApiSchema()
                    {
                        Type = "array",
                        Items = new OpenApiSchema()
                        {
                            Type = "object",
                            AdditionalProperties = new OpenApiSchema()
                            {
                                Type = "boolean"
                            }
                        }
                    },
                    ["property3"] = new OpenApiSchema()
                    {
                        Type = "string",
                        Format = "password"
                    },
                    ["property4"] = new OpenApiSchema()
                    {
                        Type = "string"
                    }
                },
                Default = new OpenApiObject()
                {
                    ["property1"] = new OpenApiArray()
                    {
                        new OpenApiInteger(12),
                        new OpenApiLong(13),
                        new OpenApiString("1"),
                    },
                    ["property2"] = new OpenApiArray()
                    {
                        new OpenApiInteger(2),
                        new OpenApiObject()
                        {
                            ["x"] = new OpenApiBoolean(true),
                            ["y"] = new OpenApiBoolean(false),
                            ["z"] = new OpenApiString("1234"),
                        }
                    },
                    ["property3"] = new OpenApiPassword("123"),
                    ["property4"] = new OpenApiDateTime(DateTime.UtcNow)
                }
            };

            // Act
            var validator = new OpenApiValidator(ValidationRuleSet.GetDefaultRuleSet());
            var walker = new OpenApiWalker(validator);
            walker.Walk(schema);

            errors = validator.Errors;
            bool result = !errors.Any();

            // Assert
            result.Should().BeFalse();
            errors.Select(e => e.Message).Should().BeEquivalentTo(new[]
            {
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
                OpenApiSchemaRules.DataTypeMismatchedErrorMessage,
            });
            errors.Select(e => e.Pointer).Should().BeEquivalentTo(new[]
            {
                "#/default/property1/0",
                "#/default/property1/2",
                "#/default/property2/0",
                "#/default/property2/1/z",
                "#/default/property4",
            });
        }
    }
}