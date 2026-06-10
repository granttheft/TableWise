const VENUES = ['Mikla Restaurant', 'Sunset Beach Club', 'Nusret Etiler', 'Lucca Bar', 'Topaz Fine Dining']

export default function SocialProof() {
  return (
    <section className="border-y border-landing-border bg-landing-surface/40 py-10">
      <div className="max-w-6xl mx-auto px-6 text-center">
        <p className="text-xs text-landing-muted uppercase tracking-widest mb-6">
          İstanbul'un önde gelen mekanları Tablewise'a güveniyor
        </p>
        <div className="flex flex-wrap items-center justify-center gap-8">
          {VENUES.map(venue => (
            <span key={venue} className="text-sm font-medium text-landing-muted/40 tracking-wide">
              {venue}
            </span>
          ))}
        </div>
      </div>
    </section>
  )
}
