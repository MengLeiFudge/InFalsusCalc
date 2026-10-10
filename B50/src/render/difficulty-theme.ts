export interface DifficultyTheme {
  difficultyIndex: number;
  label: string;
  className: string;
  accent: string;
  wash: string;
}

export const difficultyTheme: readonly DifficultyTheme[] = [
  { difficultyIndex: 0, label: "MIN", className: "hard", accent: "#91b2cc", wash: "#243541" },
  { difficultyIndex: 1, label: "EVO", className: "expert", accent: "#b09bdf", wash: "#332941" },
  { difficultyIndex: 2, label: "ULT", className: "extreme", accent: "#df8bb1", wash: "#3a2633" },
  { difficultyIndex: 3, label: "FBD", className: "special", accent: "#ff746e", wash: "#452728" },
];

export function getDifficultyTheme(difficultyIndex: number): DifficultyTheme {
  const theme = difficultyTheme[difficultyIndex];
  if (theme === undefined) throw new Error("Unsupported difficulty index: " + difficultyIndex + ".");
  return theme;
}
