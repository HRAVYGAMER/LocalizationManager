import * as vscode from 'vscode';
import { ApiClient } from '../backend/apiClient';
import { LrmService } from '../backend/lrmService';

export class StatusBarManager {
    private statusBarItem: vscode.StatusBarItem;
    private apiClient: ApiClient;
    private lrmService: LrmService;
    private updateInterval: NodeJS.Timeout | undefined;

    constructor(apiClient: ApiClient, lrmService: LrmService) {
        this.apiClient = apiClient;
        this.lrmService = lrmService;

        // Create status bar item (aligned to right, priority 100)
        this.statusBarItem = vscode.window.createStatusBarItem(
            vscode.StatusBarAlignment.Right,
            100
        );

        this.statusBarItem.command = 'lrm.showQuickActions';
        this.statusBarItem.tooltip = 'Click for LRM quick actions';
        this.statusBarItem.show();

        // Update every 30 seconds
        this.startAutoUpdate();
    }

    private startAutoUpdate(): void {
        // Initial update
        this.update();

        // Auto-update every 30 seconds
        this.updateInterval = setInterval(() => {
            this.update();
        }, 30000);
    }

    public async update(): Promise<void> {
        try {
            // Get stats from API
            const stats = await this.apiClient.getStats();

            // Calculate translation coverage from languages
            const languages = stats.languages || [];

            // Use overall coverage from API
            const avgCoverage = Math.round(stats.overallCoverage || 0);

            // Count total missing translations across all languages (excluding default)
            const totalMissing = languages
                .filter(lang => !lang.isDefault)
                .reduce((sum, lang) => sum + (lang.totalCount - lang.translatedCount), 0);

            // Determine validation status
            let validationIcon = '$(check)';
            let validationText = 'Valid';

            if (totalMissing > 10) {
                validationIcon = '$(warning)';
                validationText = `${totalMissing} missing`;
            }

            // Service status
            const serviceIcon = '$(circle-filled)';
            const serviceText = 'Running';

            // Build status bar text
            this.statusBarItem.text = `$(globe) LRM: ${avgCoverage}% | ${validationIcon} ${validationText} | ${serviceIcon} ${serviceText}`;

            // Update tooltip with detailed info
            this.statusBarItem.tooltip = this.buildTooltip(stats, avgCoverage, totalMissing);

        } catch (error) {
            // Service is down
            this.statusBarItem.text = '$(error) LRM: Failed';
            this.statusBarItem.tooltip = 'Localization Manager service is not running. Click to restart.';
            this.statusBarItem.command = 'lrm.restartBackend';
        }
    }

    private buildTooltip(stats: any, avgCoverage: number, totalMissing: number): string {
        const languages = stats.languages || [];
        const resourcePath = this.lrmService.getResourcePath() || 'Not configured';

        const lines = [
            'Localization Manager',
            '',
            `📁 Resource Folder:`,
            `  ${resourcePath}`,
            '',
            `Translation Coverage: ${avgCoverage}%`,
            `  Total Keys: ${stats.totalKeys}`,
            `  Languages: ${languages.length}`,
            `  Missing Translations: ${totalMissing}`,
            '',
            'Languages:',
            ...languages.map((lang: any) => {
                let langName: string;
                if (lang.isDefault || lang.languageCode === '' || lang.languageCode === 'default') {
                    langName = 'English (Default)';
                } else {
                    try {
                        const cultureName = new Intl.DisplayNames(['en'], { type: 'language' });
                        langName = cultureName.of(lang.languageCode) || lang.languageCode.toUpperCase();
                    } catch {
                        langName = lang.languageCode.toUpperCase();
                    }
                }
                return `  ${langName}: ${Math.round(lang.coverage)}%`;
            }),
            '',
            'Click for quick actions'
        ];

        return lines.join('\n');
    }

    public dispose(): void {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
        }
        this.statusBarItem.dispose();
    }
}
