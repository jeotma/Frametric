export interface MysteryBoxHintDto {
    boxId: string;
    hint: string;
}

export interface MysteryBoxDto {
    boxIds: string[];
    variant: number;
    generatedAt: string;
    hints?: MysteryBoxHintDto[] | null;
}
