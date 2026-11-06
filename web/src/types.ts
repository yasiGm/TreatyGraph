// These types describe the JSON that the ASP.NET Core API returns.

export type EventType = "alliance" | "war" | "coalition" | "diplomacy" | "support" | "proxy";

export interface TreatyEvent {
  t: EventType;      // kind of event
  src: string;       // data source (for example "COW Alliances v4.1")
  s: string;         // start date ("1853", "1853-03" or "1853-03-28")
  e: string | null;  // end date, or null
  on: boolean;       // still open when the dataset ends
  lc: boolean;       // started on or before 1816
  k: number;         // sort key
  title: string;
  detail: string;
}

export interface CountryMeta {
  c: number;         // Correlates of War country code
  abbr: string;
  name: string;
  spans: number[][]; // years in which the state existed
}

export interface CoverageItem {
  src: string;
  from: number;
  to: number;
  note: string;
}

export interface Meta {
  countries: CountryMeta[];
  coverage: CoverageItem[];
}

export interface Leader {
  name: string;
  s: string;
  e: string;
  exit: string;
  role: "state" | "government" | "both";
  source: string;
}

export interface War {
  name: string;
  s: string;
  e: string;
  k: number;
  result: string | null;
  initiator: boolean;
  vs: string[];
  with: string[];
  deaths: number | null;
}

export interface CivilWar {
  name: string;
  s: string;
  e: string | null;
  k: number;
  sides: string;
  internationalized: boolean;
}

export interface AllianceItem {
  title: string;
  src: string;
  s: string;
  e: string | null;
  on: boolean;
}

export interface AlliancePartner {
  partner: number;
  partnerName: string;
  items: AllianceItem[];
}

export interface CountryProfile {
  c: number;
  abbr: string;
  name: string;
  spans: number[][];
  leaders: Leader[] | null;
  wars: War[];
  civil: CivilWar[];
  alliances: AlliancePartner[];
}
