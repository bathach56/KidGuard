import 'package:flutter/material.dart';

import '../../devices/domain/device_repository.dart';
import '../../devices/domain/device_summary.dart';
import '../domain/device_log_entry.dart';

class LogListScreen extends StatefulWidget {
  const LogListScreen({
    super.key,
    this.device,
    this.accessToken,
    this.deviceRepository,
  });

  final DeviceSummary? device;
  final String? accessToken;
  final DeviceRepository? deviceRepository;

  @override
  State<LogListScreen> createState() => _LogListScreenState();
}

class _LogListScreenState extends State<LogListScreen> {
  late Future<List<DeviceLogEntry>>? _logsFuture;

  static const _sampleLogs = [
    DeviceLogEntry(
      processName: 'steam.exe',
      action: 'blocked',
      mode: 'study',
      message: 'Blocked distracting application during study mode.',
      createdAt: '10:24',
    ),
    DeviceLogEntry(
      processName: 'chrome.exe',
      action: 'opened',
      mode: 'study',
      message: 'Allowed browser activity.',
      createdAt: '10:18',
    ),
    DeviceLogEntry(
      processName: 'discord.exe',
      action: 'blocked',
      mode: 'punishment',
      message: 'Blocked chat application during strict mode.',
      createdAt: '09:42',
    ),
  ];

  bool get _usesBackend =>
      widget.device != null &&
      widget.accessToken != null &&
      widget.deviceRepository != null;

  @override
  void initState() {
    super.initState();
    _logsFuture = _usesBackend ? _loadLogs() : null;
  }

  Future<List<DeviceLogEntry>> _loadLogs() {
    return widget.deviceRepository!.getDeviceLogs(
      accessToken: widget.accessToken!,
      deviceId: widget.device!.deviceId,
    );
  }

  void _retry() {
    setState(() {
      _logsFuture = _loadLogs();
    });
  }

  @override
  Widget build(BuildContext context) {
    final subtitle = widget.device == null
        ? 'Review recent child device activity.'
        : 'Recent activity for ${widget.device!.name}.';

    return Scaffold(
      appBar: AppBar(title: const Text('Logs')),
      body: SafeArea(
        child: _usesBackend
            ? FutureBuilder<List<DeviceLogEntry>>(
                future: _logsFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState != ConnectionState.done) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  if (snapshot.hasError) {
                    return _LogListMessage(
                      icon: Icons.cloud_off_outlined,
                      title: 'Unable to load logs',
                      message: 'Check the backend connection and try again.',
                      action: OutlinedButton.icon(
                        onPressed: _retry,
                        icon: const Icon(Icons.refresh),
                        label: const Text('Retry'),
                      ),
                    );
                  }

                  final logs = snapshot.data ?? const <DeviceLogEntry>[];
                  if (logs.isEmpty) {
                    return const _LogListMessage(
                      icon: Icons.history_outlined,
                      title: 'No logs yet',
                      message:
                          'Activity logs will appear after the agent syncs.',
                    );
                  }

                  return _LogListContent(subtitle: subtitle, logs: logs);
                },
              )
            : _LogListContent(subtitle: subtitle, logs: _sampleLogs),
      ),
    );
  }
}

class _LogListContent extends StatelessWidget {
  const _LogListContent({required this.subtitle, required this.logs});

  final String subtitle;
  final List<DeviceLogEntry> logs;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;

    return ListView.separated(
      padding: const EdgeInsets.all(20),
      itemCount: logs.length + 1,
      separatorBuilder: (_, index) => SizedBox(height: index == 0 ? 16 : 10),
      itemBuilder: (context, index) {
        if (index == 0) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Activity Logs',
                style: textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                subtitle,
                style: textTheme.bodyMedium?.copyWith(
                  color: Theme.of(context).colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          );
        }

        return _LogTile(log: logs[index - 1]);
      },
    );
  }
}

class _LogTile extends StatelessWidget {
  const _LogTile({required this.log});

  final DeviceLogEntry log;

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return ListTile(
      contentPadding: const EdgeInsets.all(16),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: BorderSide(color: colorScheme.outlineVariant),
      ),
      leading: CircleAvatar(
        backgroundColor: colorScheme.secondaryContainer,
        foregroundColor: colorScheme.onSecondaryContainer,
        child: Icon(
          log.action == 'blocked'
              ? Icons.block_outlined
              : Icons.open_in_new_outlined,
        ),
      ),
      title: Text(log.processName),
      subtitle: Padding(
        padding: const EdgeInsets.only(top: 6),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(log.message),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: [
                Chip(
                  avatar: const Icon(Icons.rule_outlined, size: 18),
                  label: Text(log.action),
                  visualDensity: VisualDensity.compact,
                  materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                Chip(
                  avatar: const Icon(Icons.shield_outlined, size: 18),
                  label: Text(log.mode),
                  visualDensity: VisualDensity.compact,
                  materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
              ],
            ),
          ],
        ),
      ),
      trailing: Text(log.createdAt),
    );
  }
}

class _LogListMessage extends StatelessWidget {
  const _LogListMessage({
    required this.icon,
    required this.title,
    required this.message,
    this.action,
  });

  final IconData icon;
  final String title;
  final String message;
  final Widget? action;

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
            Icon(icon, size: 48, color: colorScheme.primary),
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
            if (action != null) ...[const SizedBox(height: 16), action!],
          ],
        ),
      ),
    );
  }
}
