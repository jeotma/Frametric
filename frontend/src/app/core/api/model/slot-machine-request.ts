export interface SlotMachineRequest {
    scope: number;
    genre?: string | null;
    decade?: number | null;
    director?: string | null;
    duration?: string | null;
    country?: string | null;
    customSourceIds?: string[] | null;
    customSourceTitles?: string[] | null;
}
