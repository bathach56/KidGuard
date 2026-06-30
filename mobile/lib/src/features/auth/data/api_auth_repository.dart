import '../../../core/api/api_client.dart';
import '../domain/auth_repository.dart';
import '../domain/auth_session.dart';

class ApiAuthRepository implements AuthRepository {
  const ApiAuthRepository(this._apiClient);

  final ApiClient _apiClient;

  @override
  Future<AuthSession> login({
    required String email,
    required String password,
  }) async {
    final response = await _apiClient.post<AuthSession>(
      '/auth/login',
      body: {'email': email, 'password': password},
      parseData: AuthSession.fromJson,
    );

    return response.data;
  }
}
