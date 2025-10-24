using UnityEngine;

public class DampedSpring
{
    public static void CalcDampedSpringMotionParams(ref DampedSpringMotionParams outParams, float deltaTime, float angularFrequency, float dampingRatio)
    {
        const float epsilon = 0.0001f;

        // Clamp to valid range
        dampingRatio = Mathf.Max(dampingRatio, 0f);
        angularFrequency = Mathf.Max(angularFrequency, 0f);

        // No oscillation if frequency is zero
        if (angularFrequency < epsilon)
        {
            outParams.posPosCoef = 1f; outParams.posVelCoef = 0f;
            outParams.velPosCoef = 0f; outParams.velVelCoef = 1f;
            return;
        }

        if (dampingRatio > 1f + epsilon)
        {
            // Over-damped case
            float za = -angularFrequency * dampingRatio;
            float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
            float z1 = za - zb;
            float z2 = za + zb;

            float e1 = Mathf.Exp(z1 * deltaTime);
            float e2 = Mathf.Exp(z2 * deltaTime);

            float invTwoZb = 1f / (2f * zb); // = 1 / (z2 - z1)

            float e1OverTwoZb = e1 * invTwoZb;
            float e2OverTwoZb = e2 * invTwoZb;

            float z1e1OverTwoZb = z1 * e1OverTwoZb;
            float z2e2OverTwoZb = z2 * e2OverTwoZb;

            outParams.posPosCoef = e1OverTwoZb * z2 - z2e2OverTwoZb + e2;
            outParams.posVelCoef = -e1OverTwoZb + e2OverTwoZb;

            outParams.velPosCoef = (z1e1OverTwoZb - z2e2OverTwoZb + e2) * z2;
            outParams.velVelCoef = -z1e1OverTwoZb + z2e2OverTwoZb;
        }
        else if (dampingRatio < 1f - epsilon)
        {
            // Under-damped case
            float omegaZeta = angularFrequency * dampingRatio;
            float alpha = angularFrequency * Mathf.Sqrt(1f - dampingRatio * dampingRatio);

            float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
            float cosTerm = Mathf.Cos(alpha * deltaTime);
            float sinTerm = Mathf.Sin(alpha * deltaTime);

            float invAlpha = 1f / alpha;

            float expSin = expTerm * sinTerm;
            float expCos = expTerm * cosTerm;
            float expOmegaZetaSinOverAlpha = expTerm * omegaZeta * sinTerm * invAlpha;

            outParams.posPosCoef = expCos + expOmegaZetaSinOverAlpha;
            outParams.posVelCoef = expSin * invAlpha;

            outParams.velPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSinOverAlpha;
            outParams.velVelCoef = expCos - expOmegaZetaSinOverAlpha;
        }
        else
        {
            // Critically damped case
            float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
            float timeExp = deltaTime * expTerm;
            float timeExpFreq = timeExp * angularFrequency;

            outParams.posPosCoef = timeExpFreq + expTerm;
            outParams.posVelCoef = timeExp;

            outParams.velPosCoef = -angularFrequency * timeExpFreq;
            outParams.velVelCoef = -timeExpFreq + expTerm;
        }
    }

    public static bool UpdateDampedSpringMotion(ref float pos, ref float vel, float equilibriumPos, DampedSpringMotionParams param)
    {
        if (Mathf.Abs(pos - equilibriumPos) < 0.001f && Mathf.Abs(vel) < 0.001f)
        {
            pos = equilibriumPos;
            vel = 0f;
            return false;
        }
        
        float oldPos = pos - equilibriumPos;
        float oldVel = vel;

        pos = oldPos * param.posPosCoef + oldVel * param.posVelCoef + equilibriumPos;
        vel = oldPos * param.velPosCoef + oldVel * param.velVelCoef;
        return true;
    }
}

public struct DampedSpringMotionParams
{
    public float posPosCoef;        //How much old position affects new position        position update
    public float posVelCoef;        //How much old velocity affects new position        position update
    public float velPosCoef;        //How much old position affects new velocity        velocity update
    public float velVelCoef;        //How much old velocity affects new velocity        velocity update
}