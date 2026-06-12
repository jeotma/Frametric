export interface DiceRollRequest {
    scope: number;
    diceTypes?: number[] | null;
    customSourceIds?: string[] | null;
    customSourceTitles?: string[] | null;
}
