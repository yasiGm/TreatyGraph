import type { CountryProfile, Meta, TreatyEvent } from "./types";

async function getJSON<T>(url: string): Promise<T> {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`${url} returned ${response.status}`);
  }
  return response.json() as Promise<T>;
}

export const getMeta = () => getJSON<Meta>("/api/meta");

export const getCountry = (c: number) => getJSON<CountryProfile>(`/api/countries/${c}`);

// The API expects the smaller country code first.
export const getPair = (a: number, b: number) =>
  getJSON<TreatyEvent[]>(`/api/pairs/${Math.min(a, b)}/${Math.max(a, b)}`);
