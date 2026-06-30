import 'package:flutter/material.dart';

import '../../../core/api/api_exception.dart';
import '../domain/device_repository.dart';
import '../domain/paired_device.dart';
import 'device_list_screen.dart';

class PairDeviceScreen extends StatefulWidget {
  const PairDeviceScreen({
    super.key,
    required this.accessToken,
    required this.deviceRepository,
  });

  final String accessToken;
  final DeviceRepository deviceRepository;

  @override
  State<PairDeviceScreen> createState() => _PairDeviceScreenState();
}

class _PairDeviceScreenState extends State<PairDeviceScreen> {
  final _formKey = GlobalKey<FormState>();
  final _pairCodeController = TextEditingController();
  bool _isSubmitting = false;
  String? _errorMessage;
  PairedDevice? _pairedDevice;

  @override
  void dispose() {
    _pairCodeController.dispose();
    super.dispose();
  }

  Future<void> _pairDevice() async {
    if (!_formKey.currentState!.validate() || _isSubmitting) {
      return;
    }

    setState(() {
      _isSubmitting = true;
      _errorMessage = null;
      _pairedDevice = null;
    });

    try {
      final pairedDevice = await widget.deviceRepository.pairDevice(
        accessToken: widget.accessToken,
        pairCode: _pairCodeController.text,
      );

      if (!mounted) {
        return;
      }

      setState(() {
        _pairedDevice = pairedDevice;
      });
    } on ApiException catch (exception) {
      if (!mounted) {
        return;
      }
      setState(() {
        _errorMessage = exception.message;
      });
    } catch (_) {
      if (!mounted) {
        return;
      }
      setState(() {
        _errorMessage = 'Unable to pair device.';
      });
    } finally {
      if (mounted) {
        setState(() {
          _isSubmitting = false;
        });
      }
    }
  }

  void _openDevices() {
    Navigator.of(context).pushReplacement(
      MaterialPageRoute<void>(
        builder: (_) => DeviceListScreen(
          accessToken: widget.accessToken,
          deviceRepository: widget.deviceRepository,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    final textTheme = Theme.of(context).textTheme;

    return Scaffold(
      appBar: AppBar(title: const Text('Pair Device')),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            Text(
              'Connect Windows Agent',
              style: textTheme.headlineSmall?.copyWith(
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 6),
            Text(
              'Enter the pair code shown by the Windows Agent.',
              style: textTheme.bodyMedium?.copyWith(
                color: colorScheme.onSurfaceVariant,
              ),
            ),
            const SizedBox(height: 24),
            Form(
              key: _formKey,
              child: TextFormField(
                controller: _pairCodeController,
                enabled: !_isSubmitting,
                textCapitalization: TextCapitalization.characters,
                decoration: const InputDecoration(
                  labelText: 'Pair Code',
                  prefixIcon: Icon(Icons.link_outlined),
                  border: OutlineInputBorder(),
                ),
                validator: (value) {
                  final pairCode = value?.trim() ?? '';
                  if (pairCode.isEmpty) {
                    return 'Pair code is required.';
                  }
                  if (pairCode.length != 8) {
                    return 'Pair code must be 8 characters.';
                  }
                  return null;
                },
              ),
            ),
            if (_errorMessage != null) ...[
              const SizedBox(height: 12),
              Text(
                _errorMessage!,
                style: textTheme.bodyMedium?.copyWith(color: colorScheme.error),
              ),
            ],
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: _isSubmitting ? null : _pairDevice,
              icon: _isSubmitting
                  ? const SizedBox.square(
                      dimension: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.check_circle_outline),
              label: Text(_isSubmitting ? 'Pairing' : 'Pair Device'),
            ),
            if (_pairedDevice != null) ...[
              const SizedBox(height: 24),
              _PairSuccessCard(
                pairedDevice: _pairedDevice!,
                onOpenDevices: _openDevices,
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _PairSuccessCard extends StatelessWidget {
  const _PairSuccessCard({
    required this.pairedDevice,
    required this.onOpenDevices,
  });

  final PairedDevice pairedDevice;
  final VoidCallback onOpenDevices;

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
            Row(
              children: [
                Icon(Icons.verified_outlined, color: colorScheme.primary),
                const SizedBox(width: 10),
                Expanded(
                  child: Text(
                    'Device paired',
                    style: textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            _PairInfoRow(label: 'Device', value: pairedDevice.name),
            _PairInfoRow(label: 'Mode', value: pairedDevice.mode),
            _PairInfoRow(label: 'Device ID', value: pairedDevice.deviceId),
            _PairInfoRow(
              label: 'Device Token',
              value: pairedDevice.deviceToken,
            ),
            const SizedBox(height: 16),
            OutlinedButton.icon(
              onPressed: onOpenDevices,
              icon: const Icon(Icons.devices_outlined),
              label: const Text('View Devices'),
            ),
          ],
        ),
      ),
    );
  }
}

class _PairInfoRow extends StatelessWidget {
  const _PairInfoRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final textTheme = Theme.of(context).textTheme;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 5),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: textTheme.labelLarge?.copyWith(fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: 2),
          SelectableText(value.isEmpty ? '--' : value),
        ],
      ),
    );
  }
}
