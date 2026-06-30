import 'package:flutter/material.dart';

import '../../logs/presentation/log_list_screen.dart';
import '../domain/device_summary.dart';
import 'device_list_screen.dart';

class DeviceDetailScreen extends StatefulWidget {
  const DeviceDetailScreen({super.key, required this.device});

  final DeviceSummary device;

  @override
  State<DeviceDetailScreen> createState() => _DeviceDetailScreenState();
}

class _DeviceDetailScreenState extends State<DeviceDetailScreen> {
  late String _selectedMode;

  static const _modes = ['fun', 'study', 'punishment'];

  @override
  void initState() {
    super.initState();
    _selectedMode = widget.device.mode;
  }

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;
    final device = widget.device;

    return Scaffold(
      appBar: AppBar(title: const Text('Device Detail')),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
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
                            label: _selectedMode,
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
                _InfoRow(label: 'Current Mode', value: _selectedMode),
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
                    selected: {_selectedMode},
                    onSelectionChanged: (selection) {
                      setState(() {
                        _selectedMode = selection.first;
                      });
                    },
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _InfoSection(
              title: 'Recent Activity',
              children: [
                const _InfoRow(
                  label: 'Last Log',
                  value: 'No activity synced yet',
                ),
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
