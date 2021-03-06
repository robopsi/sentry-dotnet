using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sentry.Internals;
using Sentry.Protocol;
using Xunit;

namespace Sentry.Tests.Protocol
{
    public class BreadcrumbTests : ImmutableTests<Breadcrumb>
    {
        [Fact]
        public void SerializeObject_ParameterlessConstructor_IncludesTimestamp()
        {
            var sut = new Breadcrumb("test", "unit");

            var actualJson = JsonSerializer.SerializeObject(sut);
            var actual = JsonSerializer.DeserializeObject(actualJson);

            DateTimeOffset actualTimestamp = actual.timestamp;

            Assert.NotEqual(default, actualTimestamp);
        }

        [Fact]
        public void SerializeObject_AllPropertiesSetToNonDefault_SerializesValidObject()
        {
            var sut =  new Breadcrumb(
                timestamp: DateTimeOffset.MaxValue,
                message: "message1",
                type: "type1",
                data: ImmutableDictionary<string, string>.Empty.Add("key", "val"),
                category: "category1",
                level: BreadcrumbLevel.Warning);

            var actual = JsonSerializer.SerializeObject(sut);

            Assert.Equal("{\"timestamp\":\"9999-12-31T23:59:59.9999999+00:00\","
                        + "\"message\":\"message1\","
                        + "\"type\":\"type1\","
                        + "\"data\":{\"key\":\"val\"},"
                        + "\"category\":\"category1\","
                        + "\"level\":\"warning\"}",
                actual);
        }

        [Theory]
        [MemberData(nameof(TestCases))]
        public void SerializeObject_TestCase_SerializesAsExpected((Breadcrumb breadcrumb, string serialized) @case)
        {
            var actual = JsonSerializer.SerializeObject(@case.breadcrumb);

            Assert.Equal(@case.serialized, actual);
        }

        public static IEnumerable<object[]> TestCases()
        {
            // Timestamp is included in every breadcrumb
            var expectedTimestamp = DateTimeOffset.MaxValue;
            var expectedTimestampString = "9999-12-31T23:59:59.9999999+00:00";
            var timestampString = $"\"timestamp\":\"{expectedTimestampString}\"";

            yield return new object[] { (new Breadcrumb (expectedTimestamp), $"{{{timestampString}}}") };
            yield return new object[] { (new Breadcrumb (expectedTimestamp, message: "message"), $"{{{timestampString},\"message\":\"message\"}}") };
            yield return new object[] { (new Breadcrumb (expectedTimestamp, type: "type"), $"{{{timestampString},\"type\":\"type\"}}") };
            yield return new object[] { (new Breadcrumb (expectedTimestamp, data: ImmutableDictionary<string, string>.Empty.Add("key", "val")), $"{{{timestampString},\"data\":{{\"key\":\"val\"}}}}") };
            yield return new object[] { (new Breadcrumb (expectedTimestamp, category: "category"), $"{{{timestampString},\"category\":\"category\"}}") };
            yield return new object[] { (new Breadcrumb (expectedTimestamp, level: BreadcrumbLevel.Critical), $"{{{timestampString},\"level\":\"critical\"}}") };
        }
    }
}
