import type { ReactNode } from "react";
import type { Counts } from "../events";

interface Props {
  counts: Counts;
  warsNote?: string;
}

// Simple line icons (24x24, drawn with the colour of the card).
const Icon = ({ children }: { children: ReactNode }) => (
  <svg viewBox="0 0 24 24" width="26" height="26" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    {children}
  </svg>
);

const ICONS = {
  // handshake: an alliance
  alliance: (
    <Icon>
      <path d="m11 17 2 2a1 1 0 1 0 3-3" />
      <path d="m14 14 2.5 2.5a1 1 0 1 0 3-3l-3.88-3.88a3 3 0 0 0-4.24 0l-.88.88a1 1 0 1 1-3-3l2.81-2.81a5.79 5.79 0 0 1 7.06-.87l.47.28a2 2 0 0 0 1.42.25L21 4" />
      <path d="m21 3 1 11h-2" />
      <path d="M3 3 2 14l6.5 6.5a1 1 0 1 0 3-3" />
      <path d="M3 4h8" />
    </Icon>
  ),
  // crossed swords: war
  war: (
    <Icon>
      <path d="M14.5 17.5 3 6V3h3l11.5 11.5" />
      <path d="M13 19l6-6" />
      <path d="M16 16l4 4" />
      <path d="M19 21l2-2" />
      <path d="M14.5 6.5 18 3h3v3l-3.5 3.5" />
      <path d="M5 14l4 4" />
      <path d="M7 17l-3 3" />
      <path d="M3 19l2 2" />
    </Icon>
  ),
  // bird (peace dove): diplomacy
  diplomacy: (
    <Icon>
      <path d="M16 7h.01" />
      <path d="M3.4 18H12a8 8 0 0 0 8-8V7a4 4 0 0 0-7.28-2.3L2 20" />
      <path d="m20 7 2 .5-2 .5" />
      <path d="M10 18v3" />
      <path d="M14 17.75V21" />
      <path d="M7 18a6 6 0 0 0 3.84-10.61" />
    </Icon>
  ),
  // people side by side: fighting on the same side
  coalition: (
    <Icon>
      <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </Icon>
  ),
} as const;

export function StatCards({ counts, warsNote }: Props) {
  const cards = [
    { key: "alliance", label: "Alliances", note: "alliance records", color: "brand" },
    { key: "war", label: "Wars", note: warsNote ?? "wars against each other", color: "orange" },
    { key: "diplomacy", label: "Diplomacy", note: "diplomatic changes seen", color: "purple" },
    { key: "coalition", label: "Side by side", note: "wars on the same side", color: "blue" },
  ] as const;

  return (
    <div className="stats">
      {cards.map((card) => (
        <div key={card.key} className={`card stat ${card.color}`}>
          <div className="stat-head">
            <span className="stat-icon">{ICONS[card.key]}</span>
            <span className="stat-label">{card.label}</span>
          </div>
          <strong className="stat-number">{counts[card.key]}</strong>
          <span className="stat-note">{card.note}</span>
        </div>
      ))}
    </div>
  );
}
