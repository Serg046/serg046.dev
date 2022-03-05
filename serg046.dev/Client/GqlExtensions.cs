﻿using System.Net.Http.Json;
using System.Text.Json;

namespace serg046.dev.Client
{
	public static class GqlExtensions
	{
		public static Task<T?> ReadFromGqlAsync<T>(this HttpContent content, T model, JsonSerializerOptions? options = null)
		{
			var x = content.ReadFromJsonAsync<T>();
			return x;
		}
	}
}
