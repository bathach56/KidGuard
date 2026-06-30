import 'package:flutter/material.dart';

import '../../logs/presentation/log_list_screen.dart';
import '../domain/device_repository.dart';
import '../domain/device_summary.dart';
import 'device_list_screen.dart';

class DeviceDetailScreen extends StatefulWidget {
  const DeviceDetailScreen({
    super.key,
    required this.initialDevice,
    required this.accessToken,
    required this.deviceRepository,
  });

  final DeviceSummary initialDevice;
  final String accessToken;
  final DeviceRepository deviceRepository;

  @override
  State<DeviceDetailScreen> createState() => _DeviceDetailScreenState();
}

class _DeviceDetailScreenState extends State<DeviceDetailScreen> {
  late Future<DeviceSummary> _deviceFuture;
  late String _selectedMode;
  bool _hasSyncedLoadedMode = false;

  static const _modes = ['fun', 'study', 'punishment'];

  @override
  void initState() {
    super.initState();
    _selectedMode = widget.initialDevice.mode;
    _deviceFuture = _loadDevice();
  }

  Future<DeviceSummary> _loadDevice() {
    return widget.deviceRepository.getDevice(
      accessToken: widget.accessToken,
      deviceId: widget.initialDevice.deviceId,
    );
  }

  void _retry() {
    setState(() {
      _hasSyncedLoadedMode = false;
      _deviceFuture = _loadDevice();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Device Detail')),
      body: SafeArea(
        child: FutureBuilder<DeviceSummary>(
          future: _deviceFuture,
          initialData: widget.initialDevice,
          builder: (context, snapshot) {
            if (snapshot.hasError) {
              return _DeviceDetailMessage(
                title: 'Unable to load device',
                message: 'Check the backend connection and try again.',
                onRetry: _retry,
              );
            }

            final device = snapshot.data ?? widget.initialDevice;
            if (snapshot.connectionState == ConnectionState.done &&
                !_hasSyncedLoadedMode &&
                _modes.contains(device.mode)) {
              _selectedMode = device.mode;
              _hasSyncedLoadedMode = true;
            }

            return _DeviceDetailContent(
              device: device,
              selectedMode: _selectedMode,
              isRefreshing: snapshot.connectionState != ConnectionState.done,
              onModeChanged: (mode) {
                setState(() {
                  _selectedMode = mode;
                });
              },
            );
          },
        ),
      ),
    );
  }
}

class _DeviceDetailContent extends StatelessWidget {
  const _DeviceDetailContent({
    required this.device,
    required this.selectedMode,
    required this.isRefreshing,
    required this.onModeChanged,
  });

  final DeviceSummary device;
  final String selectedMode;
  final bool isRefreshing;
  final ValueChanged<String> onModeChanged;

  static const _modes = ['fun', 'study', 'punishment'];

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return ListView(
      padding: const EdgeInsets.all(20),
      children: [
        if (isRefreshing) const LinearProgressIndicator(),
        if (isRefreshing) const SizedBox(height: 16),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            CircleAvatar(
              radius: 28,
              backgroundColor: device.isOnline
                  ? colorScheme.primaryContainer
                  : colorScheme.surfaceContainerHighest,
              foregroundColor: device.isOnline
                  ? colorScheme.onPrimaryContainer
                  : colorScheme.onSurfaceVariant,
              child: const Icon(Icons.computer_outlined, size: 30),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    device.name,
                    style: textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(device.computerName),
                  const SizedBox(height: 12),
                  Wrap(
                    spacing: 8,
                    runSpacing: 8,
                    children: [
                      StatusChip(
                        label: device.isOnline ? 'online' : 'offline',
                        icon: device.isOnline
                            ? Icons.wifi_tethering_outlined
                            : Icons.wifi_off_outlined,
                      ),
                      StatusChip(
                        label: selectedMode,
                        icon: Icons.shield_outlined,
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
        const SizedBox(height: 28),
        _InfoSection(
          title: 'Protection Status',
          children: [
            _InfoRow(label: 'Current Mode', value: selectedMode),
            _InfoRow(label: 'Connection', value: device.lastSeen),
            const _InfoRow(label: 'Agent Version', value: '0.0.1-demo'),
          ],
        ),
        const SizedBox(height: 16),
        _InfoSection(
          title: 'Mode Control',
          children: [
            Align(
              alignment: Alignment.centerLeft,
              child: SegmentedButton<String>(
                segments: _modes
                    .map(
                      (mode) => ButtonSegment<String>(
                        value: mode,
                        label: Text(mode),
                        icon: const Icon(Icons.shield_outlined),
                      ),
                    )
                    .toList(),
                selected: {selectedMode},
                onSelectionChanged: (selection) {
                  onModeChanged(selection.first);
                },
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        _InfoSection(
          title: 'Recent Activity',
          children: [
            const _InfoRow(label: 'Last Log', value: 'No activity synced yet'),
            const _InfoRow(label: 'Blocked Apps', value: '0 today'),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute<void>(
                    builder: (_) => LogListScreen(device: device),
                  ),
                );
              },
              icon: const Icon(Icons.history_outlined),
              label: const Text('View Logs'),
            ),
          ],
        ),
      ],
    );
  }
}

class _DeviceDetailMessage extends StatelessWidget {
  const _DeviceDetailMessage({
    required this.title,
    required this.message,
    required this.onRetry,
  });

  final String title;
  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.cloud_off_outlined,
              size: 48,
              color: colorScheme.primary,
            ),
            const SizedBox(height: 16),
            Text(
              title,
              textAlign: TextAlign.center,
              style: textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 8),
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            OutlinedButton.icon(
              onPressed: onRetry,
              icon: const Icon(Icons.refresh),
              label: const Text('Retry'),
            ),
          ],
        ),
      ),
    );
  }
}

class _InfoSection extends StatelessWidget {
  const _InfoSection({required this.title, required this.children});

  final String title;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return DecoratedBox(
      decoration: BoxDecoration(
        border: Border.all(color: colorScheme.outlineVariant),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 12),
            ...children,
          ],
        ),
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Text(
              label,
              style: textTheme.bodyMedium?.copyWith(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(child: Text(value, textAlign: TextAlign.end)),
        ],
      ),
    );
  }
}
