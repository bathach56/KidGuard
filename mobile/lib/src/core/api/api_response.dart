class ApiResponse<T> {
  const ApiResponse({
    required this.success,
    required this.message,
    required this.data,
    this.errors = const [],
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Object? json) parseData,
  ) {
    final rawErrors = json['errors'];

    return ApiResponse<T>(
      success: json['success'] == true,
      message: json['message']?.toString() ?? '',
      data: parseData(json['data']),
      errors: rawErrors is List
          ? rawErrors.map((item) => item.toString()).toList()
          : const [],
    );
  }

  final bool success;
  final String message;
  final T data;
  final List<String> errors;
}
