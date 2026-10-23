using System;
using System.Collections.Generic;
using UnityEngine;
namespace MahjongSlide.Withdrawal
{
    internal sealed class MahjongCashCounter
    {
        internal sealed class Flight
        {
            internal MahjongCashCounter Owner;
            internal decimal Amount;
            internal float Deadline;
            internal bool Completed;
            internal void Release()
            {
                if (Completed) return;
                Completed = true;
                Owner.Release(this);
            }
        }

        readonly List<Flight> flights = new List<Flight>();
        decimal balance, held, from, target;
        float startedAt, duration;
        bool animating;
        internal decimal Displayed { get; private set; }
        internal bool Initialized { get; private set; }
        internal bool IsAnimating => animating;

        internal void Clear()
        {
            foreach (var flight in flights) flight.Completed = true;
            flights.Clear();
            held = 0m;
            animating = Initialized = false;
        }

        internal void SetBalance(decimal value, float seconds, float now)
        {
            value = System.Math.Max(0m, value);
            duration = Mathf.Max(0f, seconds);
            if (!Initialized || value < balance || duration <= 0f)
            {
                Clear();
                Initialized = true;
                balance = from = target = Displayed = value;
                return;
            }
            balance = value;
            Retarget(now);
        }

        internal Flight Hold(decimal amount, float timeout, float now, bool alreadyCredited)
        {
            if (!Initialized || duration <= 0f || timeout <= 0f || amount <= 0m) return null;
            var flight = new Flight { Owner = this, Amount = amount, Deadline = now + timeout };
            flights.Add(flight);
            held += amount;
            // F1 reserves before credit, F2's source schedules its flight just
            // after credit in the same call stack. Neither may roll digits back.
            if (alreadyCredited) Retarget(now);
            return flight;
        }

        void Release(Flight flight)
        {
            if (!flights.Remove(flight)) return;
            held = System.Math.Max(0m, held - flight.Amount);
            Retarget(Time.unscaledTime);
        }

        void Retarget(float now)
        {
            decimal next = System.Math.Max(Displayed, balance - held);
            if (next == target) return;
            from = Displayed;
            target = next;
            startedAt = now;
            animating = target > from;
        }

        internal bool Tick(float now)
        {
            for (int index = flights.Count - 1; index >= 0; index--)
                if (now >= flights[index].Deadline) flights[index].Release();
            if (!animating) return false;
            float progress = duration <= 0f ? 1f : Mathf.Clamp01((now - startedAt) / duration);
            decimal eased = (decimal)Mathf.SmoothStep(0f, 1f, progress);
            Displayed = from + (target - from) * eased;
            if (progress >= 1f) { Displayed = target; animating = false; }
            return true;
        }
    }
}
