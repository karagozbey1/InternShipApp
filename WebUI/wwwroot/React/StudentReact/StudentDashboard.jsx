import Header from "./Header.jsx";
import Sidebar from "./Sidebar.jsx";
import Stats from "./Stats.jsx";
import Activities from "./Activities.jsx";

export default function StudentDashboard() {
    return (
        <>
            <Header />
            <div style={{ display: "grid", gridTemplateColumns: "260px 1fr", gap: "20px", margin: "20px" }}>
                <Sidebar />
                <main>
                    <Stats />
                    <div className="card" style={{ marginTop: "20px" }}>
                        <h2>Staj Süreci</h2>
                        <div style={{ background: "#e5e7eb", height: "10px", borderRadius: "10px" }}>
                            <div style={{ background: "#10b981", width: "100%", height: "100%" }}></div>
                        </div>
                    </div>
                    <Activities />
                </main>
            </div>
        </>
    );
}
