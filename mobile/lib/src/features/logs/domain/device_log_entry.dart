class DeviceLogEntry {
  const DeviceLogEntry({
    required this.processName,
    required this.action,
    required this.mode,
    required this.message,
    required this.createdAt,
  });

  factory DeviceLogEntry.fromJson(Object? json) {
    if (json is! Map<String, dynamic>) {
      throw const FormatException('Invalid log response.');
    }

    return DeviceLogEntry(
      processName: json['processName']?.toString() ?? 'unknown.exe',
      action: json['action']?.toString() ?? 'unknown',
      mode: json['mode']?.toString() ?? 'fun',
      message: json['message']?.toString() ?? 'No message.',
      createdAt: json['createdAt']?.toString() ?? '',
    );
  }

  final String processName;
  final String action;
  final String mode;
  final String message;
  final String createdAt;
}
