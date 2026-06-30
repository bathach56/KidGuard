import 'dart:convert';

import 'package:http/http.dart' as http;

import 'api_config.dart';
import 'api_exception.dart';
import 'api_response.dart';

class ApiClient {
  ApiClient({required this.config, http.Client? httpClient})
    : _httpClient = httpClient ?? http.Client();

  final ApiConfig config;
  final http.Client _httpClient;

  Future<ApiResponse<T>> post<T>(
    String path, {
    Map<String, Object?>? body,
    String? bearerToken,
    required T Function(Object? json) parseData,
  }) async {
    final response = await _httpClient.post(
      config.endpoint(path),
      headers: _headers(bearerToken),
      body: jsonEncode(body ?? const <String, Object?>{}),
    );

    return _decodeResponse(response, parseData);
  }

  Future<ApiResponse<T>> get<T>(
    String path, {
    Map<String, String>? queryParameters,
    String? bearerToken,
    required T Function(Object? json) parseData,
  }) async {
    final response = await _httpClient.get(
      config.endpoint(path, queryParameters),
      headers: _headers(bearerToken),
    );

    return _decodeResponse(response, parseData);
  }

  Future<ApiResponse<T>> put<T>(
    String path, {
    Map<String, Object?>? body,
    String? bearerToken,
    required T Function(Object? json) parseData,
  }) async {
    final response = await _httpClient.put(
      config.endpoint(path),
      headers: _headers(bearerToken),
      body: jsonEncode(body ?? const <String, Object?>{}),
    );

    return _decodeResponse(response, parseData);
  }

  Map<String, String> _headers(String? bearerToken) {
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      if (bearerToken != null && bearerToken.isNotEmpty)
        'Authorization': 'Bearer $bearerToken',
    };
  }

  ApiResponse<T> _decodeResponse<T>(
    http.Response response,
    T Function(Object? json) parseData,
  ) {
    final decoded = jsonDecode(response.body);
    if (decoded is! Map<String, dynamic>) {
      throw const ApiException('Unexpected API response.');
    }

    final apiResponse = ApiResponse<T>.fromJson(decoded, parseData);
    if (response.statusCode < 200 ||
        response.statusCode >= 300 ||
        !apiResponse.success) {
      throw ApiException(apiResponse.message, errors: apiResponse.errors);
    }

    return apiResponse;
  }
}
