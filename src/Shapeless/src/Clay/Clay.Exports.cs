﻿// 版权归百小僧及百签科技（广东）有限公司所有。
// 
// 此源代码遵循位于源代码树根目录中的 LICENSE 文件的许可证。

namespace Shapeless;

/// <summary>
///     流变对象
/// </summary>
public sealed partial class Clay
{
    /// <summary>
    ///     <inheritdoc cref="Clay" />
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    public Clay(ClayOptions? options = null)
        : this(ClayType.Object, options)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="Clay" />
    /// </summary>
    /// <param name="clayType">
    ///     <see cref="ClayType" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    public Clay(ClayType clayType, ClayOptions? options = null)
    {
        // 初始化 ClayOptions
        Options = options ?? ClayOptions.Default;

        // 创建 JsonObject 实例并指示属性名称是否不区分大小写
        JsonCanvas = JsonNode.Parse(clayType is ClayType.Object ? "{}" : "[]",
            new JsonNodeOptions { PropertyNameCaseInsensitive = Options.PropertyNameCaseInsensitive })!;

        IsObject = clayType is ClayType.Object;
        IsArray = clayType is ClayType.Array;
    }

    /// <summary>
    ///     字符串索引
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    public object? this[string keyOrIndex]
    {
        get => GetValue(keyOrIndex);
        set => SetValue(keyOrIndex, value);
    }

    /// <summary>
    ///     字符索引
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    public object? this[char keyOrIndex]
    {
        get => GetValue(keyOrIndex);
        set => SetValue(keyOrIndex, value);
    }

    /// <summary>
    ///     整数索引
    /// </summary>
    /// <param name="index">索引</param>
    public object? this[int index]
    {
        get => GetValue(index);
        set => SetValue(index, value);
    }

    /// <summary>
    ///     是否是单一对象
    /// </summary>
    public bool IsObject { get; }

    /// <summary>
    ///     是否是集合（数组）
    /// </summary>
    public bool IsArray { get; }

    /// <summary>
    ///     元素数量
    /// </summary>
    public int Count => IsObject ? JsonCanvas.AsObject().Count : JsonCanvas.AsArray().Count;

    /// <summary>
    ///     是否为空元素
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    ///     获取键或索引数组
    /// </summary>
    public object[] Indexes =>
        (IsObject ? EnumerateObject().Select(object (u) => u.Key) : EnumerateArray().Select(object (u) => u.Key))
        .ToArray();

    /// <summary>
    ///     获取值数组
    /// </summary>
    public dynamic?[] Values =>
        (IsObject ? EnumerateObject().Select(u => u.Value) : EnumerateArray().Select(u => u.Value)).ToArray();

    // /// <summary>
    // ///     反序列化时没有匹配的属性字典集合
    // /// </summary>
    // [JsonExtensionData]
    // public Dictionary<object, object?> Extensions { get; set; } = new();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     返回循环访问元素的枚举数
    /// </summary>
    /// <returns>
    ///     <see cref="IEnumerator{T}" />
    /// </returns>
    public IEnumerator<KeyValuePair<object, dynamic?>> GetEnumerator() =>
        IsObject
            ? EnumerateObject().Select(u => new KeyValuePair<object, dynamic?>(u.Key, u.Value)).GetEnumerator()
            : EnumerateArray().Select(u => new KeyValuePair<object, dynamic?>(u.Key, u.Value)).GetEnumerator();

    /// <summary>
    ///     创建空对象类型的 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay EmptyObject(ClayOptions? options = null) => new(options);

    /// <summary>
    ///     创建空数组类型的 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay EmptyArray(ClayOptions? options = null) => new(ClayType.Array, options);

    /// <summary>
    ///     将对象转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="obj">
    ///     <see cref="object" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <param name="jsonDocumentOptions">
    ///     <see cref="JsonDocumentOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay Parse(object? obj, ClayOptions? options = null,
        JsonDocumentOptions jsonDocumentOptions = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(obj);

        // 初始化 ClayOptions 实例
        var clayOptions = options ?? ClayOptions.Default;

        // 初始化 JsonNodeOptions 实例
        var jsonNodeOptions =
            new JsonNodeOptions { PropertyNameCaseInsensitive = clayOptions.PropertyNameCaseInsensitive };

        // 将对象转换为 JsonNode 实例
        var jsonNode = obj switch
        {
            string rawJson => JsonNode.Parse(rawJson, jsonNodeOptions, jsonDocumentOptions),
            Stream utf8Json => JsonNode.Parse(utf8Json, jsonNodeOptions, jsonDocumentOptions),
            byte[] utf8JsonBytes => JsonNode.Parse(utf8JsonBytes, jsonNodeOptions, jsonDocumentOptions),
            _ => SerializeToNode(obj, clayOptions)
        };

        return new Clay(jsonNode, clayOptions);
    }

    /// <summary>
    ///     将 <see cref="Utf8JsonReader" /> 转换为 <see cref="Clay" /> 实例
    /// </summary>
    /// <param name="utf8JsonReader">
    ///     <see cref="Utf8JsonReader" />
    /// </param>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <param name="jsonDocumentOptions">
    ///     <see cref="JsonDocumentOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static Clay Parse(ref Utf8JsonReader utf8JsonReader, ClayOptions? options = null,
        JsonDocumentOptions jsonDocumentOptions = default) =>
        Parse(utf8JsonReader.GetRawText(), options, jsonDocumentOptions);

    /// <summary>
    ///     检查键或索引是否定义
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Contains(object keyOrIndex)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(keyOrIndex);

        // 将索引转换为字符串类型
        var stringIndex = keyOrIndex.ToString()!;

        // 检查是否是单一对象
        if (IsObject)
        {
            return JsonCanvas.AsObject().ContainsKey(stringIndex);
        }

        // 尝试将字符串索引转换为整数索引
        if (int.TryParse(stringIndex, out var intIndex))
        {
            return intIndex >= 0 && intIndex < JsonCanvas.AsArray().Count;
        }

        return false;
    }

    /// <summary>
    ///     根据键或索引获取值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? Get(object keyOrIndex) => GetValue(keyOrIndex);

    /// <summary>
    ///     根据键或索引获取目标类型的值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? Get(object keyOrIndex, Type resultType, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 根据键或索引查找 JsonNode 节点
        var jsonNode = FindNode(keyOrIndex);

        return resultType == typeof(Clay)
            ? new Clay(jsonNode, Options)
            : jsonNode.As(resultType, jsonSerializerOptions ?? Options.JsonSerializerOptions);
    }

    /// <summary>
    ///     根据键或索引获取目标类型的值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    public TResult? Get<TResult>(object keyOrIndex, JsonSerializerOptions? jsonSerializerOptions = null) =>
        (TResult?)Get(keyOrIndex, typeof(TResult), jsonSerializerOptions);

    /// <summary>
    ///     获取 <see cref="JsonNode" /> 实例
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <returns>
    ///     <see cref="JsonNode" />
    /// </returns>
    public JsonNode? GetNode(object keyOrIndex) => FindNode(keyOrIndex);

    /// <summary>
    ///     根据键或索引设置值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="value">值</param>
    public void Set(object keyOrIndex, object? value) => SetValue(keyOrIndex, value);

    /// <summary>
    ///     在指定索引处插入值
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="index">索引</param>
    /// <param name="value">值</param>
    /// <exception cref="NotSupportedException"></exception>
    public bool Insert(int index, object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(Insert));

        // 检查数组索引合法性
        EnsureLegalArrayIndex(index, out _);

        // 将 JsonCanvas 转换为 JsonArray 实例
        var jsonArray = JsonCanvas.AsArray();

        // 获取 JsonArray 长度
        var count = jsonArray.Count;

        // 检查索引大于数组长度
        if (index > count)
        {
            return SetValue(index, value);
        }

        // 在指定位置插入
        jsonArray.Insert(index, SerializeToNode(value, Options));
        return true;
    }

    /// <summary>
    ///     根据键或索引删除数据
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool Remove(object keyOrIndex) => RemoveValue(keyOrIndex);

    /// <summary>
    ///     将 <see cref="Clay" /> 转换为目标类型
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    public object? As(Type resultType, JsonSerializerOptions? jsonSerializerOptions = null) =>
        resultType == typeof(Clay)
            ? this
            : JsonCanvas.As(resultType, jsonSerializerOptions ?? Options.JsonSerializerOptions);

    /// <summary>
    ///     将 <see cref="Clay" /> 转换为目标类型
    /// </summary>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    public TResult? As<TResult>(JsonSerializerOptions? jsonSerializerOptions = null) =>
        (TResult?)As(typeof(TResult), jsonSerializerOptions);

    /// <summary>
    ///     深度克隆
    /// </summary>
    /// <param name="options">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public Clay DeepClone(ClayOptions? options = null) => new Clay(JsonCanvas.DeepClone()).Rebuilt(options);

    /// <summary>
    ///     删除所有键或索引
    /// </summary>
    public void Clear()
    {
        // 检查是否是单一对象
        if (IsObject)
        {
            JsonCanvas.AsObject().Clear();
        }
        else
        {
            JsonCanvas.AsArray().Clear();
        }
    }

    /// <summary>
    ///     写入提供的 <see cref="Utf8JsonWriter" /> 作为 JSON
    /// </summary>
    /// <param name="writer">
    ///     <see cref="Utf8JsonWriter" />
    /// </param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    public void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions? jsonSerializerOptions = null) =>
        JsonCanvas.WriteTo(writer, jsonSerializerOptions ?? Options.JsonSerializerOptions);

    /// <summary>
    ///     尝试根据键或索引获取值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool TryGet(object keyOrIndex, out object? value)
    {
        // 检查键或索引是否定义
        if (Contains(keyOrIndex))
        {
            value = Get(keyOrIndex);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    ///     尝试根据键或索引获取目标类型的值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="resultType">转换的目标类型</param>
    /// <param name="value">值</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool TryGet(object keyOrIndex, Type resultType, out object? value,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 检查键或索引是否定义
        if (Contains(keyOrIndex))
        {
            value = Get(keyOrIndex, resultType, jsonSerializerOptions);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    ///     尝试根据键或索引获取目标类型的值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="value">值</param>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool TryGet<TResult>(object keyOrIndex, out TResult? value,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 检查键或索引是否定义
        if (Contains(keyOrIndex))
        {
            value = Get<TResult>(keyOrIndex, jsonSerializerOptions);
            return true;
        }

        value = (TResult?)(object?)null;
        return false;
    }

    /// <summary>
    ///     尝试根据键或索引设置值
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="JsonException"></exception>
    public bool TrySet(object keyOrIndex, object? value)
    {
        try
        {
            Set(keyOrIndex, value);
            return true;
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     尝试在指定索引处插入值
    /// </summary>
    /// <remarks>当 <see cref="IsArray" /> 为 <c>true</c> 时有效。</remarks>
    /// <param name="index">索引</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="JsonException"></exception>
    public bool TryInsert(int index, object? value)
    {
        // 检查是否是单一对象实例调用
        ThrowIfMethodCalledOnSingleObject(nameof(TryInsert));

        // 检查数组索引合法性
        EnsureLegalArrayIndex(index, out _);

        try
        {
            return Insert(index, value);
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     尝试根据键或索引删除数据
    /// </summary>
    /// <param name="keyOrIndex">键或索引</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    public bool TryRemove(object keyOrIndex) => Contains(keyOrIndex) && RemoveValue(keyOrIndex);

    /// <inheritdoc />
    public override string ToString() => JsonCanvas.ToString();

    /// <summary>
    ///     将 <see cref="Clay" /> 输出为 JSON 格式字符串
    /// </summary>
    /// <remarks>性能通常比 <see cref="ToString" /> 方式略高。</remarks>
    /// <param name="jsonSerializerOptions">
    ///     <see cref="JsonSerializerOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string ToJsonString(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // 获取提供的 JSON 序列化选项或默认选项
        var serializerOptions = jsonSerializerOptions ?? Options.JsonSerializerOptions;

        // 如果指定了命名策略，则对 JsonCanvas 进行键名转换；否则直接使用原 JsonCanvas
        var jsonCanvasToSerialize = serializerOptions.PropertyNamingPolicy is not null
            ? JsonCanvas.TransformKeysWithNamingPolicy(serializerOptions.PropertyNamingPolicy)
            : JsonCanvas;

        // 空检查
        ArgumentNullException.ThrowIfNull(jsonCanvasToSerialize);

        return jsonCanvasToSerialize.ToJsonString(serializerOptions);
    }

    /// <summary>
    ///     将 <see cref="Clay" /> 输出为 XML 格式字符串
    /// </summary>
    /// <param name="xmlWriterSettings">
    ///     <see cref="XmlWriterSettings" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public string ToXmlString(XmlWriterSettings? xmlWriterSettings = null)
    {
        // 初始化 Utf8StringWriter 实例
        using var stringWriter = new Utf8StringWriter();

        // 初始化 XmlWriter 实例
        // 注意：如果使用 using var xmlWriter = ...; 代码方式，则需要手动调用 xmlWriter.Flush(); 方法来确保所有数据都被写入
        using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
        {
            // 将 XElement 的内容保存到 XmlWriter 中
            As<XElement>()?.Save(xmlWriter);
        }

        return stringWriter.ToString();
    }
}