﻿// 版权归百小僧及百签科技（广东）有限公司所有。
// 
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace Shapeless.Tests;

public class ClayTests(ITestOutputHelper output)
{
    [Fact]
    public void New_Invalid_Parameters() => Assert.Throws<ArgumentNullException>(() => new Clay((JsonNode?)null));

    [Fact]
    public void New_ReturnOK()
    {
        var clay = new Clay(JsonNode.Parse("{\"id\":1,\"name\":\"Furion\"}"));
        Assert.NotNull(clay.Options);
        Assert.NotNull(clay.JsonCanvas);
        Assert.True(clay.IsObject);
        Assert.False(clay.IsArray);

        var clay2 = new Clay(JsonNode.Parse("[]"));
        Assert.NotNull(clay2.Options);
        Assert.NotNull(clay2.JsonCanvas);
        Assert.False(clay2.IsObject);
        Assert.True(clay2.IsArray);

        Assert.NotNull(Clay._getCSharpInvokeMemberBinderTypeArguments);
        Assert.NotNull(Clay._getCSharpInvokeMemberBinderTypeArguments.Value);

        var clay3 = new Clay(JsonValue.Create(true));
        Assert.NotNull(clay3.JsonCanvas);
        Assert.Equal("{\"data\":true}", clay3.JsonCanvas.ToJsonString());

        var clay4 = new Clay(JsonValue.Create("furion"), new ClayOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(clay4.JsonCanvas);
        Assert.Equal("{\"data\":\"furion\"}", clay4.JsonCanvas.ToJsonString());
        Assert.Equal("furion", clay4["Data"]);
    }

    [Fact]
    public void GetNodeFromObject_Invalid_Parameters()
    {
        var clay = new Clay();
        var exception = Assert.Throws<KeyNotFoundException>(() => clay.GetNodeFromObject("Name"));
        Assert.Equal("The property `Name` was not found in the Clay.", exception.Message);
    }

    [Fact]
    public void GetNodeFromObject_ReturnOK()
    {
        dynamic clay = new Clay();
        clay.Name = "Furion";

        Clay clayObject = clay;
        var jsonNode = clayObject.GetNodeFromObject("Name");
        Assert.NotNull(jsonNode);
        Assert.Equal("Furion", jsonNode.GetValue<string>());
        Assert.Throws<KeyNotFoundException>(() => clay.GetNodeFromObject("name"));
        Assert.Throws<KeyNotFoundException>(() => clay.GetNodeFromObject("Age"));

        var clay2 = new Clay(new ClayOptions { AllowMissingProperty = true });
        var jsonNode2 = clay2.GetNodeFromObject("Name");
        Assert.Null(jsonNode2);

        dynamic clay3 = new Clay(new ClayOptions { PropertyNameCaseInsensitive = true });
        clay3.Name = "Furion";
        Clay clayObject2 = clay3;

        var jsonNode3 = clayObject2.GetNodeFromObject("Name");
        Assert.NotNull(jsonNode3);
        Assert.Equal("Furion", jsonNode3.GetValue<string>());

        var jsonNode4 = clayObject2.GetNodeFromObject("name");
        Assert.NotNull(jsonNode4);
        Assert.Equal("Furion", jsonNode4.GetValue<string>());

        dynamic clay4 = new Clay();
        Assert.Throws<KeyNotFoundException>(() => clay4.GetNodeFromObject("name?"));

        dynamic clay5 = new Clay(new ClayOptions { AllowMissingProperty = true, AutoCreateNestedObjects = true });
        JsonNode jsonNode6 = clay5.GetNodeFromObject("name?");
        Assert.NotNull(jsonNode6);
        Assert.Equal("{}", jsonNode6.ToJsonString());
    }

    [Fact]
    public void GetNodeFromArray_Invalid_Parameters()
    {
        dynamic clay = new Clay(ClayType.Array);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(-1));
        Assert.Equal(
            "Negative indices are not allowed. Index must be greater than or equal to 0. (Parameter 'index')",
            exception.Message);

        var exception2 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(0));
        Assert.Equal("Index `0` is out of range. The array is empty, so no indices are valid. (Parameter 'index')",
            exception2.Message);

        clay[0] = "Furion";
        var exception3 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(1));
        Assert.Equal("Index `1` is out of range. The array contains a single element at index 0. (Parameter 'index')",
            exception3.Message);

        clay[1] = "Furion";
        var exception4 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(2));
        Assert.Equal("Index `2` is out of range. The allowed index range for the array is 0 to 1. (Parameter 'index')",
            exception4.Message);

        var exception5 = Assert.Throws<InvalidOperationException>(() => clay.GetNodeFromArray("name"));
        Assert.Equal("The provided index `name` is not a valid array index.", exception5.Message);
    }

    [Fact]
    public void GetNodeFromArray_ReturnOK()
    {
        dynamic clay = new Clay(ClayType.Array);
        clay[0] = "Furion";

        Clay clayArray = clay;
        var jsonNode = clayArray.GetNodeFromArray(0);
        Assert.NotNull(jsonNode);
        Assert.Equal("Furion", jsonNode.GetValue<string>());

        Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => clay.GetNodeFromArray(2));

        var clay2 = new Clay(ClayType.Array, new ClayOptions { AllowIndexOutOfRange = true });
        Assert.Null(clay2.GetNodeFromArray(0));
        Assert.Null(clay2.GetNodeFromArray(1));

        var clay3 = new Clay(ClayType.Array,
            new ClayOptions
            {
                AllowIndexOutOfRange = true, AutoCreateNestedArrays = true, AutoExpandArrayWithNulls = true
            });
        Assert.Equal("[]", clay3.GetNodeFromArray(2)!.ToJsonString());
    }

    [Fact]
    public void FindNode_Invalid_Parameters()
    {
        var clay = new Clay();
        Assert.Throws<ArgumentNullException>(() => clay.FindNode(null!));
    }

    [Fact]
    public void FindNode_ReturnOK()
    {
        dynamic clay = new Clay();
        clay.Name = "Furion";

        var jsonNode = ((Clay)clay).FindNode("Name");
        Assert.NotNull(jsonNode);
        Assert.Equal("Furion", jsonNode.GetValue<string>());

        dynamic clay2 = new Clay(ClayType.Array);
        clay2[0] = "Furion";
        var jsonNode2 = ((Clay)clay2).FindNode(0);
        Assert.NotNull(jsonNode2);
        Assert.Equal("Furion", jsonNode2.GetValue<string>());
    }

    [Fact]
    public void SerializeToNode_Invalid_Parameters()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("id", typeof(int));
        dataTable.Columns.Add("name", typeof(string));
        dataTable.Rows.Add(1, "Furion");
        dataTable.Rows.Add(2, "百小僧");

        Assert.Throws<NotSupportedException>(() => Clay.SerializeToNode(dataTable));
    }

    [Fact]
    public void Rebuilt_ReturnOK()
    {
        dynamic clay = new Clay();
        clay.Name = "Furion";
        Assert.Equal("Furion", clay.Name);
        Assert.Throws<KeyNotFoundException>(() => clay.name);
        _ = ((Clay)clay).Rebuilt();

        var clayOptions = new ClayOptions { PropertyNameCaseInsensitive = true };
        _ = ((Clay)clay).Rebuilt(clayOptions);
        Assert.Equal("Furion", clay.name);
        Assert.Equal(clayOptions, clay.Options);
    }

    [Fact]
    public void SerializeToNode_ReturnOK()
    {
        Assert.Null(Clay.SerializeToNode(null));
        var jsonNode = Clay.SerializeToNode(JsonNode.Parse("{}"));
        Assert.NotNull(jsonNode);
        Assert.Equal("{}", jsonNode.ToJsonString());

        dynamic clay = new Clay();
        clay.Name = "Furion";
        var jsonNode2 = Clay.SerializeToNode(clay);
        Assert.NotEqual(((Clay)clay).JsonCanvas, jsonNode2);

        var obj = new { id = 1, name = "Furion" };
        var jsonNode3 = Clay.SerializeToNode(obj);
        Assert.NotNull(jsonNode3);
        Assert.Equal("{\"id\":1,\"name\":\"Furion\"}", jsonNode3.ToJsonString());

        var dataTable = new DataTable();
        dataTable.Columns.Add("id", typeof(int));
        dataTable.Columns.Add("name", typeof(string));
        dataTable.Rows.Add(1, "Furion");
        dataTable.Rows.Add(2, "百小僧");

        var clayOptions = new ClayOptions();
        clayOptions.JsonSerializerOptions.Converters.Add(new CustomDataTableJsonConverter());

        var jsonNode4 = Clay.SerializeToNode(dataTable, clayOptions);
        Assert.NotNull(jsonNode4);
        Assert.Equal("[{\"id\":1,\"name\":\"Furion\"},{\"id\":2,\"name\":\"百小僧\"}]",
            jsonNode4.ToJsonString(clayOptions.JsonSerializerOptions));

        using var jsonDocument = JsonDocument.Parse("{\"id\":1,\"name\":\"Furion\"}");
        var jsonNode5 = Clay.SerializeToNode(jsonDocument.RootElement);
        Assert.NotNull(jsonNode5);
        Assert.Equal("{\"id\":1,\"name\":\"Furion\"}", jsonNode5.ToJsonString());
    }

    [Fact]
    public void DeserializeNode_ReturnOK()
    {
        Assert.Null(Clay.DeserializeNode(null));
        Assert.Equal("Furion", Clay.DeserializeNode(JsonNode.Parse("\"Furion\"")));
        Assert.Equal(10, Clay.DeserializeNode(JsonNode.Parse("10")));
        Assert.Equal(true, Clay.DeserializeNode(JsonNode.Parse("true")));
        Assert.Equal(false, Clay.DeserializeNode(JsonNode.Parse("false")));

        var obj = Clay.DeserializeNode(JsonNode.Parse("{\"id\":1,\"name\":\"Furion\"}"));
        Assert.NotNull(obj);
        Assert.True(obj is Clay);
        Assert.True(((Clay)obj).IsObject);

        var array = Clay.DeserializeNode(JsonNode.Parse("[1,2,3]"));
        Assert.NotNull(array);
        Assert.True(array is Clay);
        Assert.True(((Clay)array).IsArray);

        var obj2 = Clay.DeserializeNode(JsonNode.Parse("{\"id\":1,\"name\":\"Furion\"}"),
            new ClayOptions { PropertyNameCaseInsensitive = true, AllowMissingProperty = true }) as Clay;
        Assert.NotNull(obj2);
        Assert.NotNull(obj2.Options);
        Assert.True(obj2.Options.PropertyNameCaseInsensitive);
        Assert.True(obj2.Options.AllowMissingProperty);

        var obj3 = Clay.DeserializeNode(JsonNode.Parse("\"2024/12/29 3:34:30\""));
        Assert.True(obj3 is string);

        var obj4 = Clay.DeserializeNode(JsonNode.Parse("\"2024/12/29 3:34:30\""),
            new ClayOptions { AutoConvertToDateTime = true });
        Assert.True(obj4 is DateTime);
    }

    [Fact]
    public void SetNodeInObject_ReturnOK()
    {
        var clay = new Clay();
        clay.SetNodeInObject("Name", "Furion");

        var findNode = clay.FindNode("Name");
        Assert.NotNull(findNode);
        Assert.Equal("Furion", findNode.GetValue<string>());

        var clay2 = new Clay(new ClayOptions
        {
            AllowIndexOutOfRange = true, PropertyNameCaseInsensitive = true, AllowMissingProperty = true
        });
        clay2.SetNodeInObject("Name", "Furion");
        clay.SetNodeInObject("Child", clay2);

        var findNode2 = clay.FindNode("Child");
        Assert.NotNull(findNode2);
        Assert.NotEqual(findNode2, clay2.JsonCanvas);

        var clay3 = new Clay(new ClayOptions { AllowMissingProperty = true, AutoCreateNestedObjects = true });
        clay3.SetNodeInObject("nested?", 1);
        Assert.Equal(1, clay3.FindNode("nested?")!.GetValue<int>());
        Assert.Equal(1, clay3.FindNode("nested")!.GetValue<int>());
    }

    [Fact]
    public void SetNodeInArray_Invalid_Parameters()
    {
        var clay = new Clay(ClayType.Array);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => clay.SetNodeInArray(-1, null));
        Assert.Equal(
            "Negative indices are not allowed. Index must be greater than or equal to 0. (Parameter 'index')",
            exception.Message);

        var exception2 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.SetNodeInArray(1, null));
        Assert.Equal("Index `1` is out of range. The array is empty, so no indices are valid. (Parameter 'index')",
            exception2.Message);

        clay[0] = "Furion";
        var exception3 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.SetNodeInArray(2, null));
        Assert.Equal("Index `2` is out of range. The array contains a single element at index 0. (Parameter 'index')",
            exception3.Message);

        clay[1] = "Furion";
        var exception4 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.SetNodeInArray(3, null));
        Assert.Equal("Index `3` is out of range. The allowed index range for the array is 0 to 1. (Parameter 'index')",
            exception4.Message);

        var exception5 = Assert.Throws<InvalidOperationException>(() => clay.SetNodeInArray("name", null));
        Assert.Equal("The provided index `name` is not a valid array index.", exception5.Message);
    }

    [Fact]
    public void SetNodeInArray_ReturnOK()
    {
        var clay = new Clay(ClayType.Array);
        clay.SetNodeInArray(0, 1);
        var findNode = clay.FindNode(0);
        Assert.NotNull(findNode);
        Assert.Equal(1, findNode.GetValue<int>());

        clay.SetNodeInArray(1, 2);
        var findNode2 = clay.FindNode(1);
        Assert.NotNull(findNode2);
        Assert.Equal(2, findNode2.GetValue<int>());

        clay.SetNodeInArray(0, "Furion");
        var findNode3 = clay.FindNode(0);
        Assert.NotNull(findNode3);
        Assert.Equal("Furion", findNode3.GetValue<string>());

        Assert.Throws<ArgumentOutOfRangeException>(() => clay.SetNodeInArray(3, null));

        var array = new Clay(ClayType.Array,
            new ClayOptions { AllowIndexOutOfRange = true, AutoExpandArrayWithNulls = true });
        array.SetNodeInArray(3, "Furion");
        Assert.Equal("[null,null,null,\"Furion\"]", array.ToJsonString());
        var findNode4 = array.FindNode(3);
        Assert.NotNull(findNode4);
        Assert.Equal("Furion", findNode4.GetValue<string>());

        var errorArray = new Clay(ClayType.Array,
            new ClayOptions { AllowIndexOutOfRange = true });
        errorArray.SetNodeInArray(3, null);
    }

    [Fact]
    public void ThrowIfOutOfRange_ReturnOK()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Clay.ThrowIfOutOfRange(0, 0));
        Assert.Equal("Index `0` is out of range. The array is empty, so no indices are valid. (Parameter 'index')",
            exception.Message);

        var exception2 = Assert.Throws<ArgumentOutOfRangeException>(() => Clay.ThrowIfOutOfRange(1, 1));
        Assert.Equal("Index `1` is out of range. The array contains a single element at index 0. (Parameter 'index')",
            exception2.Message);

        var exception3 = Assert.Throws<ArgumentOutOfRangeException>(() => Clay.ThrowIfOutOfRange(2, 2));
        Assert.Equal("Index `2` is out of range. The allowed index range for the array is 0 to 1. (Parameter 'index')",
            exception3.Message);
    }

    [Fact]
    public void GetValue_ReturnOK()
    {
        var clay = new Clay { ["Name"] = "Furion" };
        Assert.Equal("Furion", clay.GetValue("Name"));

        var array = new Clay(ClayType.Array) { [0] = "Furion" };
        Assert.Equal("Furion", array.GetValue(0));
    }

    [Fact]
    public void SetValue_Invalid_Parameters()
    {
        var clay = new Clay();
        Assert.Throws<ArgumentNullException>(() => clay.SetValue(null!, null));
    }

    [Fact]
    public void SetValue_ReturnOK()
    {
        var clay = new Clay();
        clay.SetValue("Name", "Furion");
        Assert.Equal("Furion", clay["Name"]);

        var array = new Clay(ClayType.Array);
        array.SetValue(0, "Furion");
        Assert.Equal("Furion", array[0]);
    }

    [Fact]
    public void ProcessNestedNullPropagationIndexKey_ReturnOK()
    {
        Assert.Equal("name?", new Clay().ProcessNestedNullPropagationIndexKey("name?"));
        Assert.Equal("name",
            new Clay(new ClayOptions { AutoCreateNestedObjects = true }).ProcessNestedNullPropagationIndexKey("name?"));
    }

    [Fact]
    public void EnsureLegalArrayIndex_Invalid_Parameters()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() => Clay.EnsureLegalArrayIndex("name", out _));
        Assert.Equal("The provided index `name` is not a valid array index.", exception.Message);

        var exception2 =
            Assert.Throws<ArgumentOutOfRangeException>(() => Clay.EnsureLegalArrayIndex(-1, out _));
        Assert.Equal("Negative indices are not allowed. Index must be greater than or equal to 0. (Parameter 'index')",
            exception2.Message);
    }

    [Fact]
    public void EnsureLegalArrayIndex_ReturnOK()
    {
        Clay.EnsureLegalArrayIndex("1", out var index);
        Assert.Equal(1, index);

        Clay.EnsureLegalArrayIndex(1, out var index2);
        Assert.Equal(1, index2);
    }

    [Fact]
    public void RemoveNodeFromObject_Invalid_Parameters()
    {
        var clay = new Clay();
        var exception = Assert.Throws<KeyNotFoundException>(() => clay.RemoveNodeFromObject("Name"));
        Assert.Equal("The property `Name` was not found in the Clay.", exception.Message);
    }

    [Fact]
    public void RemoveNodeFromObject_ReturnOK()
    {
        var clay = Clay.Parse("{\"id\":1,\"name\":\"furion\"}");
        Assert.True(clay.RemoveNodeFromObject("id"));
        Assert.True(clay.RemoveNodeFromObject("name"));
        Assert.Throws<KeyNotFoundException>(() => clay.RemoveNodeFromObject("Name"));

        dynamic clay2 = Clay.Parse("{\"id\":1,\"name\":\"furion\"}");
        Assert.Throws<KeyNotFoundException>(() => clay.RemoveNodeFromObject("Id"));
        clay2(new ClayOptions { PropertyNameCaseInsensitive = true });
        Assert.True(clay2.RemoveNodeFromObject("Id"));

        var clay3 = Clay.Parse("{\"id\":1,\"name\":\"furion\"}", new ClayOptions { AllowMissingProperty = true });
        Assert.False(clay3.RemoveNodeFromObject("Name"));
    }

    [Fact]
    public void RemoveNodeFromArray_Invalid_Parameters()
    {
        dynamic clay = new Clay(ClayType.Array);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => clay.RemoveNodeFromArray(-1));
        Assert.Equal(
            "Negative indices are not allowed. Index must be greater than or equal to 0. (Parameter 'index')",
            exception.Message);

        var exception2 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.RemoveNodeFromArray(0));
        Assert.Equal("Index `0` is out of range. The array is empty, so no indices are valid. (Parameter 'index')",
            exception2.Message);

        clay[0] = "Furion";
        var exception3 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.RemoveNodeFromArray(1));
        Assert.Equal("Index `1` is out of range. The array contains a single element at index 0. (Parameter 'index')",
            exception3.Message);

        clay[1] = "Furion";
        var exception4 = Assert.Throws<ArgumentOutOfRangeException>(() => clay.RemoveNodeFromArray(2));
        Assert.Equal("Index `2` is out of range. The allowed index range for the array is 0 to 1. (Parameter 'index')",
            exception4.Message);

        var exception5 = Assert.Throws<InvalidOperationException>(() => clay.RemoveNodeFromArray("name"));
        Assert.Equal("The provided index `name` is not a valid array index.", exception5.Message);
    }

    [Fact]
    public void RemoveNodeFromArray_ReturnOK()
    {
        var clay = Clay.Parse("[1,2,3]");
        Assert.True(clay.RemoveNodeFromArray(0));
        Assert.Equal("[2,3]", clay.ToJsonString());
        Assert.True(clay.RemoveNodeFromArray(1));
        Assert.Equal("[2]", clay.ToJsonString());

        Assert.Throws<ArgumentOutOfRangeException>(() => clay.RemoveNodeFromArray(1));

        var clay2 = Clay.Parse("[1,2,3]", new ClayOptions { AllowIndexOutOfRange = true });
        Assert.False(clay2.RemoveNodeFromArray(3));
        Assert.Equal("[1,2,3]", clay2.ToJsonString());
    }

    [Fact]
    public void RemoveValue_ReturnOK()
    {
        var clay = Clay.Parse("{\"id\":1,\"name\":\"furion\",\"arr\":[1,2,3]}");
        Assert.True(clay.RemoveValue("id"));
        Assert.True(clay.RemoveValue("name"));
        Assert.Equal("{\"arr\":[1,2,3]}", clay.ToJsonString());

        dynamic array = clay["arr"]!;
        Assert.True(array.RemoveValue(0));
        Assert.Equal("{\"arr\":[2,3]}", clay.ToJsonString());
    }

    [Fact]
    public void EnumerateObject_ReturnOK()
    {
        var clay = Clay.Parse("{\"id\":1,\"name\":\"furion\"}");
        foreach (var item in clay.EnumerateObject())
        {
            output.WriteLine(item.Key.ToString());
        }
    }

    [Fact]
    public void EnumerateArray_ReturnOK()
    {
        var clay = Clay.Parse("[1,2,3]");
        foreach (var item in clay.EnumerateArray())
        {
            output.WriteLine(item.Key.ToString());
        }
    }

    [Fact]
    public void ThrowIfMethodCalledOnSingleObject_ReturnOK()
    {
        var clay = Clay.Parse("{\"id\":1,\"name\":\"furion\"}");
        var exception = Assert.Throws<NotSupportedException>(() => clay.ThrowIfMethodCalledOnSingleObject("Method"));
        Assert.Equal("`Method` method can only be used for array or collection operations.", exception.Message);

        var clay2 = Clay.Parse("[1,2,3]");
        clay2.ThrowIfMethodCalledOnSingleObject("Method");
    }
}

public class CustomDataTableJsonConverter : JsonConverter<DataTable>
{
    /// <inheritdoc />
    public override DataTable? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DataTable value, JsonSerializerOptions options)
    {
        // 将 DataTable 转换为字典集合
        var dictList = value.AsEnumerable().Select(row =>
            row.Table.Columns.Cast<DataColumn>()
                .ToDictionary(col => col.ColumnName, col => row[col] != DBNull.Value ? row[col] : null)).ToList();

        // 序列化字典列表
        JsonSerializer.Serialize(writer, dictList, options);
    }
}