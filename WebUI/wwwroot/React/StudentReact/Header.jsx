export default function Header() {
    return (
        <div className="topbar">
            <div className="title">Ana Sayfa</div>
            <div style={{ display: "flex", gap: "10px" }}>
                <button id="themeBtn" className="iconbtn">🌙</button>
                <div style={{ position: "relative" }}>
                    <button className="iconbtn">🔔</button>
                    <span className="badge">2</span>
                </div>
                <button className="iconbtn">✉️</button>
            </div>
        </div>
    );
}
