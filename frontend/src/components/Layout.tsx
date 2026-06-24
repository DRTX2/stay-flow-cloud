import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const nav = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/reservations", label: "Reservations" },
  { to: "/rooms", label: "Rooms" },
  { to: "/guests", label: "Guests" },
  { to: "/services", label: "Services" },
  { to: "/reports", label: "Reports" },
];

export function Layout() {
  const { user, logout } = useAuth();
  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">
          StayFlow<span>Cloud</span>
        </div>
        <nav>
          {nav.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) => (isActive ? "active" : "")}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <div className="main">
        <header className="topbar">
          <div>
            {user?.tenantId && (
              <span className="tenant">Tenant: {user.tenantId}</span>
            )}
          </div>
          <div className="user">
            <span>{user?.name}</span>
            <button onClick={logout}>Sign out</button>
          </div>
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
