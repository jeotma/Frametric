export interface RouletteRequest {
    scope: number;
    persistenceThreshold?: number | null;
    customSourceIds?: string[] | null;
    customSourceTitles?: string[] | null;
}
