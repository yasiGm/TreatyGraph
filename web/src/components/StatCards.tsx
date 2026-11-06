import type { Counts } from "../events";

interface Props {
  counts: Counts;
  warsNote?: string;
}

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
          <span className="stat-label">{card.label}</span>
          <strong className="stat-number">{counts[card.key]}</strong>
          <span className="stat-note">{card.note}</span>
        </div>
      ))}
    </div>
  );
}
