"use client";

import { useActionState, type ComponentProps, type ReactNode } from "react";
import { Loader2 } from "lucide-react";
import { useFormStatus } from "react-dom";
import { Button } from "@/components/ui/button";

export interface ActionState {
  error?: string;
  success?: string;
}

type Action = (state: ActionState, formData: FormData) => Promise<ActionState>;

export function ActionForm({
  action,
  children,
  ...props
}: Omit<ComponentProps<"form">, "action"> & {
  action: Action;
  children: ReactNode;
}) {
  const [state, formAction] = useActionState(action, {});

  return (
    <form action={formAction} {...props}>
      {children}
      {(state.error || state.success) && (
        <p
          role={state.error ? "alert" : "status"}
          aria-live="polite"
          className={
            state.error ? "text-sm text-destructive" : "text-sm text-muted-foreground"
          }
        >
          {state.error ?? state.success}
        </p>
      )}
    </form>
  );
}

export function ActionSubmit({
  pendingLabel,
  children,
  ...props
}: ComponentProps<typeof Button> & { pendingLabel: string }) {
  const { pending } = useFormStatus();
  return (
    <Button
      {...props}
      type="submit"
      disabled={pending || props.disabled}
      aria-busy={pending}
    >
      {pending && <Loader2 aria-hidden="true" className="animate-spin" />}
      {pending ? pendingLabel : children}
    </Button>
  );
}
