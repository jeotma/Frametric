export interface BingoSquareDto {
    objectiveId: string;
    description: string;
    isCompleted: boolean;
    completionDate?: string | null;
    row: number;
    column: number;
}

export interface BingoGridDto {
    gridSize: number;
    squares: BingoSquareDto[];
}
