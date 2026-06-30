import '../../../core/api/api_client.dart';
import '../domain/device_repository.dart';
import '../domain/device_summary.dart';

class ApiDeviceRepository implements DeviceRepository {
  const ApiDeviceRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<List<DeviceSummary>> getDevices({required String accessToken}) async {
    final response = await _apiClient.get<List<DeviceSummary>>(
      '/devices',
      bearerToken: accessToken,
      parseData: (json) {
        if (json is! Map<String, dynamic>) {
          throw const FormatException('Invalid device list response.');
        }

        final items = json['items'];
        if (items is! List) {
          return const <DeviceSummary>[];
        }

        return items.map(DeviceSummary.fromJson).toList();
      },
    );

    return response.data;
  }
}
