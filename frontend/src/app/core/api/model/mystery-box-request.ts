export interface MysteryBoxRequest {
    scope: number;
    variant: number;
    boxCount?: number;
    customSourceIds?: string[] | null;
    customSourceTitles?: string[] | null;
}
