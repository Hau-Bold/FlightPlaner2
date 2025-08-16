export interface CoordinateRequest{
    street: string | null;
    city: string;
    postalCode: string | null;
    country: string;
    isStart?: boolean;
}