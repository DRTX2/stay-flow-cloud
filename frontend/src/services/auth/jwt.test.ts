import { describe, expect, it } from "vitest";
import { asArray, decodeJwt } from "./jwt";

function makeJwt(payload: Record<string, unknown>): string {
  const b64 = (o: unknown) =>
    btoa(JSON.stringify(o)).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
  return `${b64({ alg: "none" })}.${b64(payload)}.`;
}

describe("jwt", () => {
  it("decodes a payload", () => {
    const token = makeJwt({ name: "Ada", permission: ["a", "b"] });
    const claims = decodeJwt(token);
    expect(claims?.name).toBe("Ada");
    expect(claims?.permission).toEqual(["a", "b"]);
  });

  it("returns null for malformed tokens", () => {
    expect(decodeJwt("garbage")).toBeNull();
  });

  it("normalizes claim values to arrays", () => {
    expect(asArray("x")).toEqual(["x"]);
    expect(asArray(["x", "y"])).toEqual(["x", "y"]);
    expect(asArray(undefined)).toEqual([]);
  });
});
