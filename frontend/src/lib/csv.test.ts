import { describe, expect, it } from "vitest";
import { rowsToCsv } from "./csv";

describe("rowsToCsv", () => {
  it("serializes rows with a header", () => {
    const csv = rowsToCsv(
      [
        { name: "Alpha", price: 10 },
        { name: "Bravo", price: 20 },
      ],
      [
        { key: "name", header: "Name" },
        { key: "price", header: "Price" },
      ],
    );
    expect(csv).toBe("Name,Price\nAlpha,10\nBravo,20");
  });

  it("quotes values containing commas, quotes or newlines", () => {
    const csv = rowsToCsv([{ note: 'a, "b"' }], [{ key: "note", header: "Note" }]);
    expect(csv).toBe('Note\n"a, ""b"""');
  });
});
