class DeviceSummary {
  const DeviceSummary({
    required this.deviceId,
    required this.name,
    required this.computerName,
    required this.mode,
    required this.isOnline,
    required this.lastSeen,
  });

  factory DeviceSummary.fromJson(Object? json) {
    if (json is! Map<String, dynamic>) {
      throw const FormatException('Invalid device response.');
    }

    final deviceName = json['deviceName']?.toString() ?? 'Unknown Device';

    return DeviceSummary(
      deviceId: json['deviceId']?.toString() ?? '',
      name: deviceName,
      computerName: json['computerName']?.toString() ?? deviceName,
      mode: json['mode']?.toString() ?? 'fun',
      isOnline: json['isOnline'] == true,
      lastSeen: json['lastSeen']?.toString() ?? 'Not seen yet',
    );
  }

  final String deviceId;
  final String name;
  final String computerName;
  final String mode;
  final bool isOnline;
  final String lastSeen;
}
